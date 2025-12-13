using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ImageToASCII.Services;
using ImageToASCII.UI;
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
    private byte[]? _writeBuffer;
    private int _writeBufferSize = 0;
    private bool _writeBufferFromPool = false;

    public VideoRecorder(int fps = 30)
    {
        _fps = fps;
    }

    public async Task<bool> InitializeFFmpegAsync()
    {
        if (_isFFmpegReady)
            return true;

        string ffmpegDir = Path.Combine(AppContext.BaseDirectory, "ffmpeg");
        
        bool ok = await FFmpegBootstrapper.EnsureFFmpegAsync(ffmpegDir);
        if (!ok)
        {
            ConsoleUI.WriteError("Не удалось инициализировать FFmpeg");
            _isFFmpegReady = false;
            return false;
        }

        _isFFmpegReady = true;
        return true;
    }

    public async IAsyncEnumerable<SKBitmap> ExtractFramesStream(string inputFile, int fps = 30)
    {
        if (!_isFFmpegReady) 
            throw new InvalidOperationException("FFmpeg не инициализирован");

        string ffmpegDir = FFmpegBootstrapper.GetFFmpegPath();
        string ffprobePath = Path.Combine(ffmpegDir, "ffprobe.exe");
        string ffmpegPath = Path.Combine(ffmpegDir, "ffmpeg.exe");

        var probe = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height -of csv=p=0 \"{inputFile}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        probe.Start();
        string output = await probe.StandardOutput.ReadToEndAsync();
        await probe.WaitForExitAsync();
        
        var dims = output.Trim().Split(',');
        if (dims.Length < 2) 
            throw new Exception("Не удалось получить размеры видео");
        
        int width = int.Parse(dims[0]);
        int height = int.Parse(dims[1]);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-loglevel error -i \"{inputFile}\" -vf fps={fps} -f rawvideo -pix_fmt rgba pipe:1",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        ConsumeStreamErrors(process.StandardError);

        int frameSize = width * height * 4;
        byte[] buffer = new byte[frameSize];
        var stdout = process.StandardOutput.BaseStream;

        try
        {
            while (true)
            {
                int totalRead = 0;
                while (totalRead < frameSize)
                {
                    int read = await stdout.ReadAsync(buffer, totalRead, frameSize - totalRead);
                    if (read == 0) yield break;
                    totalRead += read;
                }

                var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                IntPtr pixelsAddr = bitmap.GetPixels();
                Marshal.Copy(buffer, 0, pixelsAddr, buffer.Length);
                bitmap.NotifyPixelsChanged();
                yield return bitmap;
            }
        }
        finally
        {
            if (!process.HasExited) process.Kill();
            process.Dispose();
        }
    }

    public void StartRecording(string outputPath, int width, int height)
    {
        if (_isRecordingStarted) return;

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        _targetWidth = width % 2 == 0 ? width : width - 1;
        _targetHeight = height % 2 == 0 ? height : height - 1;

        ConsoleUI.ShowInfo($"Запись: {_targetWidth}x{_targetHeight} @ {_fps} FPS");

        string ffmpegDir = FFmpegBootstrapper.GetFFmpegPath();
        string ffmpegPath = Path.Combine(ffmpegDir, "ffmpeg.exe");

        _outputProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y -loglevel error -f rawvideo -pix_fmt rgba -s {_targetWidth}x{_targetHeight} -r {_fps} " +
                            $"-i pipe:0 -c:v libx264 -pix_fmt yuv420p -preset medium -crf 23 \"{outputPath}\"",
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

        if (frame.ColorType != SKColorType.Rgba8888)
        {
            toWrite = new SKBitmap(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            using var canvas = new SKCanvas(toWrite);
            canvas.DrawBitmap(frame, 0, 0);
            needsDispose = true;
        }

        if (toWrite.Width != _targetWidth || toWrite.Height != _targetHeight)
        {
            var resized = toWrite.Resize(new SKImageInfo(_targetWidth, _targetHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul), SKFilterQuality.None);
            if (needsDispose) toWrite.Dispose();
            toWrite = resized;
            needsDispose = true;
        }

        try
        {
            var pixels = toWrite.GetPixels();
            int bytesCount = _targetWidth * _targetHeight * 4;

            if (_writeBuffer == null || _writeBufferSize < bytesCount)
            {
                if (_writeBuffer != null && _writeBufferFromPool)
                    ArrayPool<byte>.Shared.Return(_writeBuffer, false);

                _writeBuffer = ArrayPool<byte>.Shared.Rent(bytesCount);
                _writeBufferSize = _writeBuffer.Length;
                _writeBufferFromPool = true;
            }

            Marshal.Copy(pixels, _writeBuffer, 0, bytesCount);
            await _inputStreamOfOutputProcess.WriteAsync(new ReadOnlyMemory<byte>(_writeBuffer, 0, bytesCount));
            await _inputStreamOfOutputProcess.FlushAsync();
        }
        finally
        {
            if (needsDispose) toWrite.Dispose();
        }
    }

    public async Task StopRecordingAsync()
    {
        if (!_isRecordingStarted || _outputProcess == null) return;

        try
        {
            _inputStreamOfOutputProcess?.Close();
            await _outputProcess.WaitForExitAsync();
            ConsoleUI.ShowInfo("Запись завершена");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Ошибка остановки: {ex.Message}");
        }
        finally
        {
            _isRecordingStarted = false;
            if (_writeBuffer != null && _writeBufferFromPool)
            {
                ArrayPool<byte>.Shared.Return(_writeBuffer, false);
                _writeBuffer = null;
                _writeBufferSize = 0;
                _writeBufferFromPool = false;
            }
            _outputProcess?.Dispose();
            _outputProcess = null;
            _inputStreamOfOutputProcess = null;
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
                    {
                        ConsoleUI.WriteError($"FFmpeg: {line}");
                    }
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

        if (_writeBuffer != null && _writeBufferFromPool)
        {
            ArrayPool<byte>.Shared.Return(_writeBuffer, false);
            _writeBuffer = null;
            _writeBufferFromPool = false;
            _writeBufferSize = 0;
        }
    }
}