using System.Runtime.InteropServices;
using SkiaSharp;
using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;
using ImageToASCII.Services;
namespace ImageToASCII.Core.Processors;
public class AsciiExporter : ImageProcessorBase, IDisposable
{
    private SKPaint? _textPaint;
    private readonly SKTypeface _typeface;
    private readonly Dictionary<char, string> _stringCache = new();
    private float _lastFontSize = -1;
    private float _charWidth;
    private IReporter _reporter;
    private GlyphMaskCache? _maskCache;
    private float _maskCacheFontSize = -1;
    public AsciiExporter(BitmapToAsciiConverter converter, IReporter reporter) : base(converter)
    {
        _reporter = reporter;
        _typeface = LoadBestMonospaceTypeface();
        
        for (int i = 0; i < 256; i++)
            _stringCache[(char)i] = ((char)i).ToString();
    }
    private SKTypeface LoadBestMonospaceTypeface()
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
                _reporter.ShowInfo($"Используется шрифт: {tf.FamilyName}");
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
        _reporter.ShowInfo("Рендеринг ASCII изображения...");
        return RenderCoreWithProgress(data, fontSize);
    }

    private SKBitmap RenderCoreOptimized(AsciiPrepareResult data, int fontSize)
    {
        EnsureMaskCache(fontSize, data.AsciiChars);

        return FastGlyphRenderer.Render(
            data.AsciiChars,
            data.Colors,
            _maskCache!,
            fontSize,
            _charWidth,
            data.OutputWidth,
            data.OutputHeight);
    }

    private SKBitmap RenderCoreWithProgress(AsciiPrepareResult data, int fontSize)
    {
        EnsureMaskCache(fontSize, data.AsciiChars);

        var chars = data.AsciiChars;
        int h = chars.GetLength(0);
        int w = chars.GetLength(1);
        int total = w * h;
        int progressStep = Math.Max(1, total / 20);
        int lastProgress = 0;

        _reporter.ShowInfo("  Прогресс рендеринга: 0%   ");

        var output = FastGlyphRenderer.Render(
            data.AsciiChars,
            data.Colors,
            _maskCache!,
            fontSize,
            _charWidth,
            data.OutputWidth,
            data.OutputHeight);

        Console.WriteLine("\r  Прогресс рендеринга: 100%   ");
        _reporter.ShowSuccess("Рендеринг завершён!");
        return output;
    }

    private void EnsureMaskCache(int fontSize, char[,] chars)
    {
        GetOrCreatePaint(fontSize);

        if (_maskCache == null || Math.Abs(_maskCacheFontSize - fontSize) > 0.01f)
        {
            _maskCache?.Dispose();
            _maskCache = new GlyphMaskCache(_typeface, fontSize, _charWidth);
            _maskCacheFontSize = fontSize;
        }

        var uniqueChars = new HashSet<char>();
        int h = chars.GetLength(0);
        int w = chars.GetLength(1);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                uniqueChars.Add(chars[y, x]);

        _maskCache.Warmup(uniqueChars);
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
            _reporter.ShowHeader("--- Информация о генерации ---");
            _reporter.ShowInfo($"Оригинал: {bitmap.Width}x{bitmap.Height}px");
        }
        float fontAspectRatio = _charWidth / (float)fontSize;
        var resized = ResizeBitmap(bitmap, fontAspectRatio);
        
        var asciiChars = _asciiConverter.Convert(resized);
        int h = asciiChars.GetLength(0);
        int w = asciiChars.GetLength(1);
        if (verbose)
            _reporter.ShowInfo($"Размер сетки: {w}x{h} символов");
        
        
        int width = resized.Width;
        int height = resized.Height;
        int total = width * height;
        uint[] colors = new uint[total];
        Span<uint> colorSpan = colors.AsSpan(0, total);
        resized.ToGrayscale(colorClassifier, colorSpan);
        int outW = (int)(w * _charWidth);
        int outH = h * fontSize;
        if (verbose)
        {
            _reporter.ShowInfo($"Ширина символа: {_charWidth:F2}px, Высота: {fontSize}px");
            _reporter.ShowInfo($"Финальный холст: {outW}x{outH}px");
            Console.WriteLine();
        }
        return new AsciiPrepareResult
        {
            ResizedBitmap = resized,
            AsciiChars = asciiChars,
            OutputWidth = outW,
            OutputHeight = outH,
            Colors = colors
        };
    }
    public void Dispose()
    {
        _textPaint?.Dispose();
        _typeface?.Dispose();
        _maskCache?.Dispose();
    }
}