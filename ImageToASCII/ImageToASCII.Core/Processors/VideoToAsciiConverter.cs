using System.Diagnostics;
using ImageToASCII.ColorSystem;
using ImageToASCII.Services;
using SkiaSharp;
namespace ImageToASCII.Core.Processors;
public class VideoToAsciiConverter
{
    private readonly VideoRecorder _videoRecorder;
    private readonly AsciiExporter _asciiExporter;
    private IReporter _reporter;
    public VideoToAsciiConverter(AsciiExporter asciiExporter,FFmpegBootstrapper bootstrapper, IReporter reporter)
    {
        _reporter = reporter;
        _asciiExporter = asciiExporter;
        _videoRecorder = new VideoRecorder(bootstrapper, reporter);
    }
    public async Task InitializeAsync()
    {
        _reporter.ShowInfo("Запуск проверки FFmpeg...");
        if (!await _videoRecorder.InitializeFFmpegAsync())
            throw new InvalidOperationException("Не удалось инициализировать FFmpeg.");
    }
    public async Task Convert(string inputFile, string outputFile, IColorClassifier colorClassifier, int fps = 30)
    {
        int frameNumber = 0;
        var totalTimer = Stopwatch.StartNew();
        var frameBlockTimer = Stopwatch.StartNew(); 
        int framesInBlock = 0;
    
        bool isRecordingStarted = false;
        Console.CursorVisible = false;
    
        _reporter.ShowHeader("--- Обработка ASCII-Видео ---");
        _reporter.ShowInfo($"Файл: {Path.GetFileName(inputFile)}");
    
        try
        {
            await foreach (var inputFrame in _videoRecorder.ExtractFramesStream(inputFile, fps))
            {
                frameNumber++;
                framesInBlock++;
    
                using SKBitmap asciiFrame = _asciiExporter.GetProcessing(inputFrame, colorClassifier);
                inputFrame.Dispose();
    
                if (asciiFrame == null) continue;
    
                if (!isRecordingStarted)
                {
                    _videoRecorder.StartRecording(outputFile, asciiFrame.Width, asciiFrame.Height);
                    isRecordingStarted = true;
                }
    
                await _videoRecorder.WriteFrameAsync(asciiFrame);
    
                if (framesInBlock >= 10)
                {
                    double blockSeconds = frameBlockTimer.Elapsed.TotalSeconds;
                    
                    double currentInstantFps = blockSeconds > 0 ? framesInBlock / blockSeconds : 0;
                    double totalElapsed = totalTimer.Elapsed.TotalSeconds;
    
                    Console.Write($"\r  [>] Кадр: {frameNumber,-5} | Текущий: {currentInstantFps,5:F1} FPS | Время: {totalElapsed,6:F1}s ");
                    framesInBlock = 0;
                    frameBlockTimer.Restart();
                }
            }
    
            Console.WriteLine();
            _reporter.ShowInfo("Финализация видеофайла...");
            await _videoRecorder.StopRecordingAsync();
            _videoRecorder.MergeAudio(inputFile, outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            _reporter.ShowError($"Ошибка при конвертации: {ex.Message}");
        }
        finally
        {
            _videoRecorder.Dispose();
            Console.CursorVisible = true;
        }
    
        totalTimer.Stop();
        Console.WriteLine();
        _reporter.ShowSuccess("Обработка завершена успешно!");
        _reporter.ShowInfo($"Всего кадров: {frameNumber}");
        _reporter.ShowInfo($"Средняя скорость: {frameNumber / totalTimer.Elapsed.TotalSeconds:F2} FPS");
        _reporter.ShowInfo($"Затрачено времени: {totalTimer.Elapsed.TotalSeconds:F2} сек");
        Console.WriteLine(new string('-', 40));
    }
}