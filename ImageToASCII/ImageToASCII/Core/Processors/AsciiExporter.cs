using SkiaSharp;
using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;
using ImageToASCII.Services;
using ImageToASCII.UI;

namespace ImageToASCII.Core.Processors;

public class AsciiExporter : ImageProcessorBase, IDisposable
{
    private SKPaint _textPaint;
    private SKTypeface _typeface;
    private static readonly string[] _oneCharCache = new string[256];

    public AsciiExporter(BitmapToAsciiConverter converter) : base(converter)
    {
        _typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    }

    public SKBitmap GetProcessing(SKBitmap bmp, IColorClassifier classifier, int fontSize = 16)
    {
        var data = PrepareInternal(bmp, classifier, fontSize, verbose: false);
        return RenderCoreOptimized(data, fontSize);
    }

    public SKBitmap PrintAndSave(SKBitmap bmp, IColorClassifier classifier, int fontSize = 16)
    {
        var data = PrepareInternal(bmp, classifier, fontSize, verbose: true);
        ConsoleUI.ShowProgress("Рендеринг ASCII изображения...");
        return RenderCoreWithProgress(data, fontSize);
    }

    public void Dispose()
    {
        _textPaint?.Dispose();
        _typeface?.Dispose();
    }

    private SKBitmap RenderCoreOptimized(AsciiPrepareResult data, int fontSize)
    {
        var output = new SKBitmap(data.OutputWidth, data.OutputHeight);
        using var canvas = new SKCanvas(output);
        canvas.Clear(SKColors.Black);

        var paint = GetOrCreatePaint(fontSize);
        int w = data.AsciiChars.GetLength(1);
        int h = data.AsciiChars.GetLength(0);
        int totalChars = w * h;

        if (data.Colors == null || data.Colors.Length < totalChars)
            throw new InvalidOperationException($"Colors массив слишком мал: {data.Colors?.Length ?? 0} < {totalChars}");

        var colors = data.Colors;
        int colorIndex = 0;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                char c = data.AsciiChars[y, x];
                uint packed = colors[colorIndex];

                byte a = (byte)(packed >> 24);
                byte r = (byte)(packed >> 16);
                byte g = (byte)(packed >> 8);
                byte b = (byte)packed;

                paint.Color = new SKColor(r, g, b, a);
                canvas.DrawText(GetOneCharString(c), x * fontSize, (y + 1) * fontSize - 2, paint);
                colorIndex++;
            }
        }

        return output;
    }

    private SKBitmap RenderCoreWithProgress(AsciiPrepareResult data, int fontSize)
    {
        var output = new SKBitmap(data.OutputWidth, data.OutputHeight);
        using var canvas = new SKCanvas(output);
        canvas.Clear(SKColors.Black);

        var paint = GetOrCreatePaint(fontSize);
        int w = data.AsciiChars.GetLength(1);
        int h = data.AsciiChars.GetLength(0);
        int total = w * h;
        int progressStep = Math.Max(1, total / 10);

        if (data.Colors == null || data.Colors.Length < total)
            throw new InvalidOperationException($"Colors массив слишком мал: {data.Colors?.Length ?? 0} < {total}");

        var colors = data.Colors;
        int colorIndex = 0;
        int lastProgress = 0;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                char c = data.AsciiChars[y, x];
                uint packed = colors[colorIndex];

                byte a = (byte)(packed >> 24);
                byte r = (byte)(packed >> 16);
                byte g = (byte)(packed >> 8);
                byte b = (byte)packed;

                paint.Color = new SKColor(r, g, b, a);
                canvas.DrawText(GetOneCharString(c), x * fontSize, (y + 1) * fontSize - 2, paint);

                colorIndex++;
                if (colorIndex % progressStep == 0)
                {
                    int progress = colorIndex * 100 / total;
                    if (progress > lastProgress)
                    {
                        lastProgress = progress;
                        Console.Write($"\r  Прогресс: {progress}%   ");
                    }
                }
            }
        }

        Console.WriteLine("\r  Прогресс: 100%   ");
        ConsoleUI.WriteSuccess("Рендеринг завершён!");
        return output;
    }

    private SKPaint GetOrCreatePaint(int fontSize)
    {
        if (_textPaint == null)
        {
            _textPaint = new SKPaint
            {
                Typeface = _typeface,
                TextSize = fontSize,
                IsAntialias = false,
                SubpixelText = false,
                FilterQuality = SKFilterQuality.None
            };
        }
        return _textPaint;
    }

    private AsciiPrepareResult PrepareInternal(SKBitmap bitmap, IColorClassifier colorClassifier, int fontSize, bool verbose)
    {
        if (verbose)
        {
            Console.WriteLine();
            ConsoleUI.WriteHeader("--- Информация о генерации ---");
            ConsoleUI.WriteInfo($"Оригинал: {bitmap.Width}x{bitmap.Height}px");
        }

        var resized = ResizeBitmap(bitmap, false);

        if (verbose)
            ConsoleUI.WriteInfo($"После ресайза: {resized.Width}x{resized.Height}px");

        var asciiChars = _asciiConverter.Convert(resized);

        if (verbose)
            ConsoleUI.WriteInfo($"ASCII сетка: {asciiChars.GetLength(1)}x{asciiChars.GetLength(0)} символов");

        resized.ToGrayscale(colorClassifier);

        int outW = asciiChars.GetLength(1) * fontSize;
        int outH = asciiChars.GetLength(0) * fontSize;

        if (verbose)
        {
            ConsoleUI.WriteInfo($"Шрифт: {fontSize}px");
            ConsoleUI.WriteInfo($"Финальный размер: {outW}x{outH}px");
            Console.WriteLine();
        }

        return new AsciiPrepareResult
        {
            ResizedBitmap = resized,
            AsciiChars = asciiChars,
            OutputWidth = outW,
            OutputHeight = outH,
            Colors = BitmapExtensions.Colors
        };
    }

    private static string GetOneCharString(char c)
    {
        int idx = (c < 256) ? (byte)c : (byte)'?';
        var s = _oneCharCache[idx];
        if (s == null)
            _oneCharCache[idx] = s = new string(c, 1);
        return s;
    }
}
