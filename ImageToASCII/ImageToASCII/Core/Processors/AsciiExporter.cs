using System.Runtime.InteropServices;
using SkiaSharp;
using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;
using ImageToASCII.Services;
using ImageToASCII.UI;

namespace ImageToASCII.Core.Processors;

public class AsciiExporter : ImageProcessorBase, IDisposable
{
    private SKPaint? _textPaint;
    private readonly SKTypeface _typeface;
    private readonly Dictionary<char, string> _stringCache = new();
    private float _lastFontSize = -1;
    private float _charWidth;

    public AsciiExporter(BitmapToAsciiConverter converter) : base(converter)
    {
        _typeface = LoadBestMonospaceTypeface();
        
        for (int i = 0; i < 256; i++)
            _stringCache[(char)i] = ((char)i).ToString();
    }

    private static SKTypeface LoadBestMonospaceTypeface()
    {
        string[] candidates = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { "Consolas", "Courier New", "Lucida Console", "monospace" }
            : new[] { "DejaVu Sans Mono", "Liberation Mono", "FreeMono", "Courier New", "monospace" };

        foreach (var name in candidates)
        {
            var tf = SKTypeface.FromFamilyName(name, SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

            if (tf != null && tf.FamilyName != "Arial" && tf.FamilyName != "sans-serif"
                && tf.FamilyName != "serif")
            {
                ConsoleUI.ShowInfo($"Используется шрифт: {tf.FamilyName}");
                return tf;
            }
            tf?.Dispose();
        }

        return SKTypeface.Default;
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

    private SKBitmap RenderCoreOptimized(AsciiPrepareResult data, int fontSize)
    {
        var paint = GetOrCreatePaint(fontSize);
        var output = new SKBitmap(data.OutputWidth, data.OutputHeight);
        using var canvas = new SKCanvas(output);
        canvas.Clear(SKColors.Black);

        var chars = data.AsciiChars;
        var colors = data.Colors;
        int h = chars.GetLength(0);
        int w = chars.GetLength(1);
        int colorIndex = 0;

        paint.GetFontMetrics(out var metrics);
        float verticalOffset = -metrics.Ascent; 

        for (int y = 0; y < h; y++)
        {
            float yPos = y * fontSize + verticalOffset;

            for (int x = 0; x < w; x++)
            {
                char c = chars[y, x];
                if (c != ' ')
                {
                    paint.Color = new SKColor(colors[colorIndex]);
                    canvas.DrawText(GetCachedString(c), x * _charWidth, yPos, paint);
                }
                colorIndex++;
            }
        }

        return output;
    }

    private SKBitmap RenderCoreWithProgress(AsciiPrepareResult data, int fontSize)
    {
        var paint = GetOrCreatePaint(fontSize);
        var output = new SKBitmap(data.OutputWidth, data.OutputHeight);
        using var canvas = new SKCanvas(output);
        canvas.Clear(SKColors.Black);

        var chars = data.AsciiChars;
        var colors = data.Colors;
        int h = chars.GetLength(0);
        int w = chars.GetLength(1);
        int total = w * h;
        int progressStep = Math.Max(1, total / 20);
        int colorIndex = 0;
        int lastProgress = 0;

        paint.GetFontMetrics(out var metrics);
        float verticalOffset = -metrics.Ascent;

        for (int y = 0; y < h; y++)
        {
            float yPos = y * fontSize + verticalOffset;

            for (int x = 0; x < w; x++)
            {
                char c = chars[y, x];
                if (c != ' ')
                {
                    paint.Color = new SKColor(colors[colorIndex]);
                    canvas.DrawText(GetCachedString(c), x * _charWidth, yPos, paint);
                }

                colorIndex++;

                if (colorIndex % progressStep == 0)
                {
                    int progress = (int)((long)colorIndex * 100 / total);
                    if (progress > lastProgress)
                    {
                        lastProgress = progress;
                        Console.Write($"\r  Прогресс рендеринга: {progress}%   ");
                    }
                }
            }
        }

        Console.WriteLine("\r  Прогресс рендеринга: 100%   ");
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
                IsAntialias = false,
                FilterQuality = SKFilterQuality.None,
                TextAlign = SKTextAlign.Left
            };
        }

        if (Math.Abs(_lastFontSize - fontSize) > 0.01f)
        {
            _textPaint.TextSize = fontSize;
            _lastFontSize = fontSize;
            _charWidth = _textPaint.MeasureText("W");
        }

        return _textPaint;
    }

    private string GetCachedString(char c)
    {
        if (_stringCache.TryGetValue(c, out var s)) return s;
        
        s = c.ToString();
        _stringCache[c] = s;
        return s;
    }

    private AsciiPrepareResult PrepareInternal(SKBitmap bitmap, IColorClassifier colorClassifier, int fontSize, bool verbose)
    {
        var paint = GetOrCreatePaint(fontSize);

        if (verbose)
        {
            Console.WriteLine();
            ConsoleUI.WriteHeader("--- Информация о генерации ---");
            ConsoleUI.WriteInfo($"Оригинал: {bitmap.Width}x{bitmap.Height}px");
        }

        float fontAspectRatio = _charWidth / (float)fontSize;

        var resized = ResizeBitmap(bitmap, fontAspectRatio);
        
        var asciiChars = _asciiConverter.Convert(resized);
        int h = asciiChars.GetLength(0);
        int w = asciiChars.GetLength(1);

        if (verbose)
            ConsoleUI.WriteInfo($"Размер сетки: {w}x{h} символов");

        resized.ToGrayscale(colorClassifier);

        int outW = (int)(w * _charWidth);
        int outH = h * fontSize;

        if (verbose)
        {
            ConsoleUI.WriteInfo($"Ширина символа: {_charWidth:F2}px, Высота: {fontSize}px");
            ConsoleUI.WriteInfo($"Финальный холст: {outW}x{outH}px");
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

    public void Dispose()
    {
        _textPaint?.Dispose();
        _typeface?.Dispose();
    }
}