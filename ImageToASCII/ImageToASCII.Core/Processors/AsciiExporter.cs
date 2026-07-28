using System.Runtime.InteropServices;
using SkiaSharp;
using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;

namespace ImageToASCII.Core.Processors;

public class AsciiExporter : ImageProcessorBase, IDisposable
{
    private SKPaint? _textPaint;
    private readonly SKTypeface _typeface;
    private readonly Dictionary<char, string> _stringCache = new();
    private float _lastFONTSIZE = -1;
    private float _charWidth;
    private IReporter _reporter;
    private GlyphMaskCache? _maskCache;
    private float _maskCacheFONTSIZE = -1;
    private char[]? _warmedPaletteChars;
    private const int FONTSIZE = 12;

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

    public (int pixelWidth, int pixelHeight, int outputWidth, int outputHeight) GetAsciiSize(int originalWidth, int originalHeight)
    {
        GetOrCreatePaint(FONTSIZE);
        float fontAspectRatio = _charWidth / FONTSIZE;
        var (pixelW, pixelH) = CalculateTargetSize(originalWidth, originalHeight, fontAspectRatio);

        int outW = (int)(pixelW * _charWidth);
        int outH = pixelH * FONTSIZE;
        if (outW % 2 != 0) outW--;
        if (outH % 2 != 0) outH--;

        return (pixelW, pixelH, outW, outH);
    }

    public SKBitmap GetProcessing(SKBitmap bmp, IColorClassifier classifier)
    {
        var data = PrepareInternal(bmp, classifier,  verbose: false);
        return RenderCoreOptimized(data, FONTSIZE);
    }

    public SKBitmap PrintAndSave(SKBitmap bmp, IColorClassifier classifier)
    {
        var data = PrepareInternal(bmp, classifier,  verbose: true);
        _reporter.ShowInfo("Рендеринг ASCII изображения...");
        return RenderCoreWithProgress(data, FONTSIZE);
    }

    private SKBitmap RenderCoreOptimized(AsciiPrepareResult data, int FONTSIZE)
    {
        EnsureMaskCache(FONTSIZE, _asciiConverter.Table);
        return FastGlyphRenderer.Render(
            data.AsciiChars,
            data.GridWidth,
            data.GridHeight,
            data.Colors,
            _maskCache!,
            FONTSIZE,
            _charWidth,
            data.OutputWidth,
            data.OutputHeight);
    }

    private SKBitmap RenderCoreWithProgress(AsciiPrepareResult data, int FONTSIZE)
    {
        EnsureMaskCache(FONTSIZE, _asciiConverter.Table);
        _reporter.ShowInfo("  Прогресс рендеринга: 0%   ");
        var output = FastGlyphRenderer.Render(
            data.AsciiChars,
            data.GridWidth,
            data.GridHeight,
            data.Colors,
            _maskCache!,
            FONTSIZE,
            _charWidth,
            data.OutputWidth,
            data.OutputHeight);
        Console.WriteLine("\r  Прогресс рендеринга: 100%   ");
        _reporter.ShowSuccess("Рендеринг завершён!");
        return output;
    }

    private void EnsureMaskCache(int FONTSIZE, IReadOnlyList<char> paletteChars)
    {
        bool needNewCache = _maskCache == null || Math.Abs(_maskCacheFONTSIZE - FONTSIZE) > 0.01f;
        if (needNewCache)
        {
            _maskCache?.Dispose();
            _maskCache = new GlyphMaskCache(_typeface, FONTSIZE, _charWidth);
            _maskCacheFONTSIZE = FONTSIZE;
            _warmedPaletteChars = null;
        }
        if (_warmedPaletteChars == null || !paletteChars.SequenceEqual(_warmedPaletteChars))
        {
            _maskCache.Warmup(new HashSet<char>(paletteChars));
            _warmedPaletteChars = paletteChars.ToArray();
        }
    }

    private SKPaint GetOrCreatePaint(int FONTSIZE)
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
        if (Math.Abs(_lastFONTSIZE - FONTSIZE) > 0.01f)
        {
            _textPaint.TextSize = FONTSIZE;
            _lastFONTSIZE = FONTSIZE;
            _charWidth = _textPaint.MeasureText("W");
        }
        return _textPaint;
    }

    private unsafe AsciiPrepareResult PrepareInternal(SKBitmap bitmap, IColorClassifier colorClassifier, bool verbose)
    {
        GetOrCreatePaint(FONTSIZE);
        if (verbose)
        {
            Console.WriteLine();
            _reporter.ShowHeader("--- Информация о генерации ---");
            _reporter.ShowInfo($"Оригинал: {bitmap.Width}x{bitmap.Height}px");
        }

        float fontAspectRatio = _charWidth / (float)FONTSIZE;
        using var resized = ResizeBitmap(bitmap, fontAspectRatio);

        var asciiChars = _asciiConverter.Convert(resized, out int gridW, out int gridH);
        if (verbose)
            _reporter.ShowInfo($"Размер сетки: {gridW}x{gridH} символов");

        int width = resized.Width;
        int height = resized.Height;
        int total = width * height;
        uint[] colors = new uint[total];

        fixed (uint* colorPtr = colors)
        {
            uint* cptr = colorPtr;
            IntPtr pixelsAddr = resized.GetPixels();
            if (pixelsAddr == IntPtr.Zero)
                throw new Exception("pixelsAddr = 0");

            byte* ptr = (byte*)pixelsAddr;
            int rowBytes = resized.RowBytes;

            for (int y = 0; y < height; y++)
            {
                uint* row = (uint*)(ptr + y * rowBytes);
                for (int x = 0; x < width; x++)
                {
                    uint pixel = row[x];
                    byte b = (byte)pixel;
                    byte g = (byte)(pixel >> 8);
                    byte r = (byte)(pixel >> 16);
                    *cptr++ = colorClassifier.GetColor(r, g, b);
                }
            }
        }

        int outW = (int)(gridW * _charWidth);
        int outH = gridH * FONTSIZE;
        if (outW % 2 != 0) outW--;
        if (outH % 2 != 0) outH--;

        if (verbose)
        {
            _reporter.ShowInfo($"Ширина символа: {_charWidth:F2}px, Высота: {FONTSIZE}px");
            _reporter.ShowInfo($"Финальный холст: {outW}x{outH}px");
            Console.WriteLine();
        }

        return new AsciiPrepareResult
        {
            AsciiChars = asciiChars,
            GridWidth = gridW,
            GridHeight = gridH,
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