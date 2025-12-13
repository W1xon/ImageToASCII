using SkiaSharp;
using ImageToASCII.ColorSystem;

namespace ImageToASCII.Services;

public static class BitmapExtensions
{
    public static uint[] Colors;

    public static unsafe void ToGrayscale(this SKBitmap bitmap, IColorClassifier classifier)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        int total = width * height;

        if (Colors == null || Colors.Length < total)
            Colors = new uint[total];

        IntPtr pixelsAddr = bitmap.GetPixels();
        if (pixelsAddr == IntPtr.Zero)
            throw new Exception("pixelsAddr = 0");

        byte* ptr = (byte*)pixelsAddr;

        
        ProcessBGRA(ptr, width, height, bitmap.RowBytes, classifier);

        bitmap.NotifyPixelsChanged();
    }

    private static unsafe void ProcessBGRA(
        byte* ptr, int width, int height, int rowBytes, IColorClassifier classifier)
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
                byte a = p[3];

                Colors[idx] = classifier.GetColor(r, g, b);

                byte gray = (byte)((r * 30 + g * 59 + b * 11) / 100);

                p[0] = gray;
                p[1] = gray;
                p[2] = gray;

                idx++;
            }
        }
    }

}
