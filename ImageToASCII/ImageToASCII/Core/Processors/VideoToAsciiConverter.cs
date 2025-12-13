using System.Diagnostics;
using ImageToASCII.ColorSystem;
using ImageToASCII.UI;
using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public class VideoToAsciiConverter
{
    private readonly VideoRecorder _videoRecorder;
    private readonly AsciiExporter _asciiExporter;
    

    public VideoToAsciiConverter(AsciiExporter asciiExporter)
    {
        _asciiExporter = asciiExporter;
        _videoRecorder = new VideoRecorder();
    }

    public async Task InitializeAsync()
    {
        ConsoleUI.ShowProgress("Запуск проверки FFmpeg...");

        if (!await _videoRecorder.InitializeFFmpegAsync())
            throw new InvalidOperationException("Не удалось инициализировать FFmpeg.");
    }
    
    public async Task Convert(string inputFile, string outputFile, IColorClassifier colorClassifier, int fps = 30)
    {
        int frameNumber = 0;
        var totalTimer = Stopwatch.StartNew();
        bool isRecordingInitialized = false;
        
        Console.CursorVisible = false;
        ConsoleUI.ShowProgress($"Начинаем обработку: {Path.GetFileName(inputFile)}");
        Console.WriteLine();
        
        try 
        {
            await foreach (var inputFrame in _videoRecorder.ExtractFramesStream(inputFile, fps))
            {
                frameNumber++;
                var frameTimer = Stopwatch.StartNew();
                
                SKBitmap? asciiFrame = null;
                try
                {
                    asciiFrame = _asciiExporter.GetProcessing(inputFrame, colorClassifier);
                    
                    frameTimer.Stop();
                    long frameMs = frameTimer.ElapsedMilliseconds;
                    
                    if (asciiFrame == null) continue;
                    
                    if (!isRecordingInitialized)
                    {
                        _videoRecorder.StartRecording(outputFile, asciiFrame.Width, asciiFrame.Height);
                        isRecordingInitialized = true;
                    }
                    
                    await _videoRecorder.WriteFrameAsync(asciiFrame);
                    
                    Console.Write($"\r  Кадр {frameNumber} обработан за {frameMs}ms   ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    ConsoleUI.WriteError($"Сбой на кадре {frameNumber}: {ex.Message}");
                    break;
                }
                finally
                {
                    asciiFrame?.Dispose();
                    inputFrame.Dispose();
                }
            }
            
            await _videoRecorder.StopRecordingAsync();
        }
        finally
        {
            _videoRecorder.Dispose();
            Console.CursorVisible = true;
        }
        
        totalTimer.Stop();
        
        if (frameNumber > 0)
        {
            Console.WriteLine();
            Console.WriteLine();
            ConsoleUI.WriteSuccess($"Обработано кадров: {frameNumber}");
            ConsoleUI.WriteInfo($"Общее время: {totalTimer.Elapsed.TotalSeconds:F2} сек");
            ConsoleUI.WriteInfo($"Сохранено: {outputFile}");
        }
    }
}