using SkiaSharp;
namespace ImageToASCII.Core.Processors;
public static class FastGlyphRenderer
{
    static byte FastDiv255(int x) => (byte)((x + 1 + (x >> 8)) >> 8);
    public static unsafe SKBitmap Render(
        char[,] chars,
        uint[] colors,
        GlyphMaskCache maskCache,
        int fontSize,
        float charWidth,
        int outputWidth,
        int outputHeight)
    {
        int h = chars.GetLength(0);
        int w = chars.GetLength(1);
        int cellW = (int)Math.Ceiling(charWidth);
        var output = new SKBitmap(outputWidth, outputHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

        byte* outPtr = (byte*)output.GetPixels();
        int outRowBytes = output.RowBytes;
        int totalBytes = outRowBytes * outputHeight;
        uint bgColor = 0xFF000000;
        var pixelSpan = new Span<uint>(outPtr, totalBytes / 4);
        pixelSpan.Fill(bgColor);
        int colorIndex = 0;
        for (int y = 0; y < h; y++)
        {
            int baseOutY = y * fontSize;
            for (int x = 0; x < w; x++)
            {
                char c = chars[y, x];
                uint packedColor = colors[colorIndex];
                colorIndex++;
                if (c == ' ')
                    continue;

                var mask = maskCache.GetOrCreate(c);
                byte r = (byte)((packedColor >> 16) & 0xFF);
                byte g = (byte)((packedColor >> 8) & 0xFF);
                byte b = (byte)(packedColor & 0xFF);
                int baseOutX = x * cellW;

                for (int my = 0; my < mask.Height; my++)
                {
                    int outY = baseOutY + my;
                    if (outY >= outputHeight) break;
                    byte* outRow = outPtr + outY * outRowBytes;
                    int maskRowOffset = my * mask.Width;
                    for (int mx = 0; mx < mask.Width; mx++)
                    {
                        int outX = baseOutX + mx;
                        if (outX >= outputWidth) break;
                        byte alpha = mask.Alpha[maskRowOffset + mx];
                        if (alpha == 0) continue;
                        int pixelOffset = outX * 4;
                        if (alpha == 255)
                        {
                            outRow[pixelOffset + 0] = r;
                            outRow[pixelOffset + 1] = g;
                            outRow[pixelOffset + 2] = b;
                            outRow[pixelOffset + 3] = 255;
                        }
                        else
                        {
                            outRow[pixelOffset + 0] = FastDiv255(r * alpha);
                            outRow[pixelOffset + 1] = FastDiv255(g * alpha);
                            outRow[pixelOffset + 2] = FastDiv255(b * alpha);
                            outRow[pixelOffset + 3] = 255;
                        }
                    }
                }
            }
        }
        output.NotifyPixelsChanged();
        return output;
    }
}