using SkiaSharp;
using ImageToASCII.ColorSystem;

namespace ImageToASCII.Services;


public static class BitmapExtensions
{
    private static readonly byte[] GammaLut = BuildGammaLut(0.8f);

    public static unsafe void ToGrayscale(this SKBitmap bitmap, IColorClassifier classifier, Span<uint> colorBuffer)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        IntPtr pixelsAddr = bitmap.GetPixels();
        if (pixelsAddr == IntPtr.Zero)
            throw new Exception("pixelsAddr = 0");

        byte* ptr = (byte*)pixelsAddr;

        ProcessBGRA(ptr, width, height, bitmap.RowBytes, classifier,colorBuffer);

        bitmap.NotifyPixelsChanged();
    }

    private static unsafe void ProcessBGRA(byte* ptr, int width, int height, int rowBytes, IColorClassifier classifier, Span<uint> colorBuffer)
    {
        int idx = 0;
        for (int y = 0; y < height; y++)
        {
            byte* row = ptr + y * rowBytes;

            for (int x = 0; x < width; x++)
            {
                byte* p = row + x * 4;
                byte b = p[0];
                byte g = p[1];
                byte r = p[2];

                colorBuffer[idx] = classifier.GetColor(r, g, b);

                int grayInt = (r * 77 + g * 150 + b * 29) >> 8;
                byte finalGray = GammaLut[grayInt];

                p[0] = finalGray;
                p[1] = finalGray;
                p[2] = finalGray;

                idx++;
            }
        }
    }

    private static byte[] BuildGammaLut(float gamma)
    {
        var lut = new byte[256];

        for (int i = 0; i < 256; i++)
        {
            float normalized = i / 255f;
            float corrected = MathF.Pow(normalized, gamma);
            lut[i] = (byte)(corrected * 255f);
        }
        return lut;
    }
}