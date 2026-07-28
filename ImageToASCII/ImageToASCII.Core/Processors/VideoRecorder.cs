using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ImageToASCII.Services;
using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public class VideoRecorder : IDisposable
{
    private readonly int _fps;
    private bool _isFFmpegReady;
    private Process? _outputProcess;
    private Stream? _inputStreamOfOutputProcess;
    private bool _isRecordingStarted;
    private int _targetWidth;
    private int _targetHeight;
    private IReporter _reporter;
    private FFmpegBootstrapper _fFmpegBootstrapper;

    private static string FfmpegExe => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
    private static string FfprobeExe => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe";

    public VideoRecorder(FFmpegBootstrapper bootstrapper, IReporter reporter, int fps = 30)
    {
        _fFmpegBootstrapper = bootstrapper;
        _reporter = reporter;
        _fps = fps;
    }

    public async Task<bool> InitializeFFmpegAsync()
    {
        if (_isFFmpegReady) return true;
        if (!_fFmpegBootstrapper.IsExist)
        {
            _reporter.ShowError("FFmpeg не установлен");
            _isFFmpegReady = false;
            return false;
        }
        _isFFmpegReady = true;
        return true;
    }

    public async Task<(int width, int height)> ProbeVideoDimensionsAsync(string inputFile)
    {
        var sw = Stopwatch.StartNew();
        string ffmpegDir = _fFmpegBootstrapper.GetFFmpegPath();
        string ffprobePath = Path.Combine(ffmpegDir, FfprobeExe);

        var probe = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -show_entries stream=width,height -of csv=p=0 \"{inputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        probe.Start();
        string output = await probe.StandardOutput.ReadToEndAsync();
        await probe.WaitForExitAsync();

        var dims = output
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(parts => parts.Where(p => int.TryParse(p, out _)).Select(int.Parse).ToArray())
            .FirstOrDefault(parsedInts => parsedInts.Length >= 2);

        if (dims == null)
            throw new Exception($"Не удалось получить размеры видео. Вывод ffprobe: '{output}'");

        sw.Stop();
        _reporter.ShowInfo($"[PROF] ffprobe: {sw.Elapsed.TotalSeconds:F3}s");
        return (dims[0], dims[1]);
    }

    public async IAsyncEnumerable<SKBitmap> ExtractFramesStream(string inputFile, int fps, int targetWidth, int targetHeight)
    {
        if (!_isFFmpegReady)
            throw new InvalidOperationException("FFmpeg не инициализирован");

        string ffmpegDir = _fFmpegBootstrapper.GetFFmpegPath();
        string ffmpegPath = Path.Combine(ffmpegDir, FfmpegExe);

        int w = targetWidth;
        int h = targetHeight;
        int frameSize = w * h * 4;

        string scaleFilter = $"scale={w}:{h}:flags=fast_bilinear";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-loglevel error -i \"{inputFile}\" -vf \"fps={fps},{scaleFilter}\" -f rawvideo -pix_fmt rgba pipe:1",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        ConsumeStreamErrors(process.StandardError);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(frameSize);
        try
        {
            var stdout = process.StandardOutput.BaseStream;
            while (true)
            {
                int totalRead = 0;
                while (totalRead < frameSize)
                {
                    int read = await stdout.ReadAsync(buffer, totalRead, frameSize - totalRead);
                    if (read == 0) yield break;
                    totalRead += read;
                }

                var bitmap = new SKBitmap(w, h, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                Marshal.Copy(buffer, 0, bitmap.GetPixels(), frameSize);
                bitmap.NotifyPixelsChanged();
                yield return bitmap;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            if (!process.HasExited) process.Kill();
            process.Dispose();
        }
    }

    public void StartRecording(string outputPath, int width, int height)
    {
        if (_isRecordingStarted) return;

        string? outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        _targetWidth = width % 2 == 0 ? width : width - 1;
        _targetHeight = height % 2 == 0 ? height : height - 1;

        _reporter.ShowInfo($"Запись: {_targetWidth}x{_targetHeight} @ {_fps} FPS");

        string ffmpegDir = _fFmpegBootstrapper.GetFFmpegPath();
        string ffmpegPath = Path.Combine(ffmpegDir, FfmpegExe);

        _outputProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y -loglevel error -f rawvideo -pix_fmt rgba -s {_targetWidth}x{_targetHeight} -r {_fps} " +
                            $"-i pipe:0 -c:v libx264 -crf 23 -preset ultrafast -pix_fmt yuv420p \"{outputPath}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        _outputProcess.Start();
        _inputStreamOfOutputProcess = _outputProcess.StandardInput.BaseStream;
        _isRecordingStarted = true;
        ConsumeStreamErrors(_outputProcess.StandardError);
    }

    public async Task WriteFrameAsync(SKBitmap frame)
    {
        if (!_isRecordingStarted || _inputStreamOfOutputProcess == null || _outputProcess!.HasExited)
            throw new InvalidOperationException("FFmpeg процесс записи не активен");

        SKBitmap toWrite = frame;
        bool needsDispose = false;

        if (frame.ColorType != SKColorType.Rgba8888 ||
            frame.Width != _targetWidth || frame.Height != _targetHeight)
        {
            var info = new SKImageInfo(_targetWidth, _targetHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            toWrite = frame.Resize(info, SKFilterQuality.Low);
            needsDispose = true;
        }

        try
        {
            var pixels = toWrite.GetPixels();
            int bytesCount = _targetWidth * _targetHeight * 4;

            unsafe
            {
                var span = new ReadOnlySpan<byte>((void*)pixels, bytesCount);
                _inputStreamOfOutputProcess.Write(span);
            }
        }
        finally
        {
            if (needsDispose) toWrite.Dispose();
        }
    }

    public async Task StopRecordingAsync()
    {
        if (!_isRecordingStarted || _outputProcess == null) return;
        var sw = Stopwatch.StartNew();
        try
        {
            _inputStreamOfOutputProcess?.Close();
            await _outputProcess.WaitForExitAsync();
            sw.Stop();
            _reporter.ShowInfo($"[PROF] FFmpeg encode finalize: {sw.Elapsed.TotalSeconds:F3}s");
            _reporter.ShowInfo("Запись завершена");
        }
        catch (Exception ex)
        {
            _reporter.ShowError($"Ошибка остановки: {ex.Message}");
        }
        finally
        {
            _isRecordingStarted = false;
            _outputProcess?.Dispose();
            _outputProcess = null;
            _inputStreamOfOutputProcess = null;
        }
    }

    public async Task MergeAudio(string audioSource, string videoSource)
    {
        var sw = Stopwatch.StartNew();
        string outputVideo = Path.Combine(
            Path.GetDirectoryName(videoSource)!,
            Path.GetFileNameWithoutExtension(videoSource) + "_audio" + Path.GetExtension(videoSource)
        );
        string args = $"-hide_banner -loglevel error -i \"{audioSource}\" -i \"{videoSource}\" -c:v copy -map 0:a? -map 1:v? -shortest \"{outputVideo}\" -y";

        string ffmpegDir = _fFmpegBootstrapper.GetFFmpegPath();
        string ffmpegPath = Path.Combine(ffmpegDir, FfmpegExe);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(ffmpegPath, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            }
        };

        _reporter.ShowInfo("Склейка аудио...");
        process.Start();

        Task<string> errorOutputTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();
        sw.Stop();

        if (process.ExitCode == 0 && File.Exists(outputVideo))
        {
            File.Delete(videoSource);
            File.Move(outputVideo, videoSource);
            _reporter.ShowSuccess($"Аудио успешно склеено! [{sw.Elapsed.TotalSeconds:F2}s]");
        }
        else
        {
            string errorMessage = await errorOutputTask;
            _reporter.ShowError($"Ошибка FFmpeg (код {process.ExitCode}): {errorMessage.Trim()}");
        }
    }

    private void ConsumeStreamErrors(StreamReader reader)
    {
        Task.Run(async () =>
        {
            try
            {
                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                        _reporter.ShowError($"FFmpeg: {line}");
                }
            }
            catch { }
        });
    }

    public void Dispose()
    {
        if (_isRecordingStarted)
        {
            try
            {
                _inputStreamOfOutputProcess?.Close();
                _outputProcess?.Kill();
                _outputProcess?.Dispose();
            }
            catch { }
        }
    }
}