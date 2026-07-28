using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public static class FastGlyphRenderer
{
    static byte FastDiv255(int x) => (byte)((x + 1 + (x >> 8)) >> 8);

    private const float BrightnessBoost = 1.4f;

    public static unsafe SKBitmap Render(
        char[] chars,
        int gridWidth,
        int gridHeight,
        uint[] colors,
        GlyphMaskCache maskCache,
        int fontSize,
        float charWidth,
        int outputWidth,
        int outputHeight)
    {
        int h = gridHeight;
        int w = gridWidth;
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
            int rowBaseIdx = y * w;

            for (int x = 0; x < w; x++)
            {
                char c = chars[rowBaseIdx + x];
                uint packedColor = colors[colorIndex];
                colorIndex++;

                if (c == ' ')
                    continue;

                var mask = maskCache.GetOrCreate(c);

                byte r = (byte)Math.Min(255, ((packedColor >> 16) & 0xFF) * BrightnessBoost);
                byte g = (byte)Math.Min(255, ((packedColor >> 8) & 0xFF) * BrightnessBoost);
                byte b = (byte)Math.Min(255, (packedColor & 0xFF) * BrightnessBoost);

                int baseOutX = x * cellW;

                int maxMy = Math.Min(mask.Height, outputHeight - baseOutY);
                int maxMx = Math.Min(mask.Width, outputWidth - baseOutX);

                for (int my = 0; my < maxMy; my++)
                {
                    int outY = baseOutY + my;
                    byte* outRow = outPtr + outY * outRowBytes;
                    int maskRowOffset = my * mask.Width;

                    for (int mx = 0; mx < maxMx; mx++)
                    {
                        int outX = baseOutX + mx;
                        byte alpha = mask.Alpha[maskRowOffset + mx];
                        if (alpha == 0) continue;

                        int pixelOffset = outX * 4;

                        if (alpha == 255)
                        {
                            *(uint*)(outRow + pixelOffset) =
                                r | ((uint)g << 8) | ((uint)b << 16) | 0xFF000000;
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