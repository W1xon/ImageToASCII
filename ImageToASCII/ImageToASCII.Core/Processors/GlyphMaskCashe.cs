using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public sealed class GlyphMaskCache : IDisposable
{
    private readonly Dictionary<char, GlyphMask> _cache = new();
    private readonly SKTypeface _typeface;
    private readonly int _fontSize;
    private readonly float _charWidth;

    public GlyphMaskCache(SKTypeface typeface, int fontSize, float charWidth)
    {
        _typeface = typeface;
        _fontSize = fontSize;
        _charWidth = charWidth;
    }

    public void Warmup(IEnumerable<char> characters)
    {
        foreach (var c in characters)
        {
            if (c == ' ') continue; 
            GetOrCreate(c);
        }
    }

    public GlyphMask GetOrCreate(char c)
    {
        if (_cache.TryGetValue(c, out var cached))
            return cached;

        var mask = RenderMask(c);
        _cache[c] = mask;
        return mask;
    }

    private unsafe GlyphMask RenderMask(char c)
    {
        int w = (int)Math.Ceiling(_charWidth);
        int h = _fontSize;

        using var paint = new SKPaint
        {
            Typeface = _typeface,
            TextSize = _fontSize,
            IsAntialias = false,         
            FilterQuality = SKFilterQuality.None,
            TextAlign = SKTextAlign.Left,
            Color = SKColors.White
        };
        paint.GetFontMetrics(out var metrics);
        float baseline = -metrics.Ascent;

        using var bmp = new SKBitmap(w, h, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawText(c.ToString(), 0, baseline, paint);
        }

        var alpha = new byte[w * h];
        byte* srcPtr = (byte*)bmp.GetPixels();
        int rowBytes = bmp.RowBytes;

        for (int y = 0; y < h; y++)
        {
            byte* row = srcPtr + y * rowBytes;
            for (int x = 0; x < w; x++)
            {
                alpha[y * w + x] = row[x * 4 + 3];
            }
        }

        return new GlyphMask(alpha, w, h);
    }

    public void Dispose()
    {
        _cache.Clear();
    }
}

public sealed class GlyphMask
{
    public byte[] Alpha { get; }
    public int Width { get; }
    public int Height { get; }

    public GlyphMask(byte[] alpha, int width, int height)
    {
        Alpha = alpha;
        Width = width;
        Height = height;
    }
}