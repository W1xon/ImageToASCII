using System.Diagnostics;
using ImageToASCII.ColorSystem;
using SkiaSharp;
namespace ImageToASCII.Core.Processors;
public class VideoToAsciiConverter
{
    private readonly VideoRecorder _videoRecorder;
    private readonly AsciiExporter _asciiExporter;
    private IReporter _reporter;
    public VideoToAsciiConverter(AsciiExporter asciiExporter, IReporter reporter)
    {
        _reporter = reporter;
        _asciiExporter = asciiExporter;
        _videoRecorder = new VideoRecorder(reporter);
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
        bool isRecordingStarted = false;
        Console.CursorVisible = false;
        _reporter.ShowHeader("--- Обработка ASCII-Видео ---");
        _reporter.ShowInfo($"Файл: {Path.GetFileName(inputFile)}");
        try
        {
            await foreach (var inputFrame in _videoRecorder.ExtractFramesStream(inputFile, fps))
            {
                frameNumber++;
                using SKBitmap asciiFrame = _asciiExporter.GetProcessing(inputFrame, colorClassifier);
                inputFrame.Dispose();
                if (asciiFrame == null) continue;
                if (!isRecordingStarted)
                {
                    _videoRecorder.StartRecording(outputFile, asciiFrame.Width, asciiFrame.Height);
                    isRecordingStarted = true;
                }
                await _videoRecorder.WriteFrameAsync(asciiFrame);
                if (frameNumber % 10 == 0)
                {
                    double elapsed = totalTimer.Elapsed.TotalSeconds;
                    double currentFps = frameNumber / elapsed;
                    Console.Write($"\r  [>] Кадр: {frameNumber,-5} | Скорость: {currentFps,5:F1} FPS | Время: {elapsed,6:F1}s ");
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