using ImageToASCII.Core.Converters;
using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public abstract class ImageProcessorBase
{
    protected readonly BitmapToAsciiConverter _asciiConverter;
    public int AsciiWidth { get; set; } = 350;

    protected ImageProcessorBase(BitmapToAsciiConverter converter)
    {
        _asciiConverter = converter;
    }

    protected SKBitmap ResizeBitmap(SKBitmap bitmap, float fontAspectRatio)
    {
        if (bitmap.Width == 0 || bitmap.Height == 0)
            throw new ArgumentException("Битмап имеет нулевой размер.");

        double imageRatio = bitmap.Width / (double)bitmap.Height;
        int targetWidth = AsciiWidth;
        int targetHeight = (int)((targetWidth / imageRatio) * fontAspectRatio);
        if (targetHeight <= 0) targetHeight = 1;

        var info = new SKImageInfo(targetWidth, targetHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        return bitmap.Resize(info, SKFilterQuality.Low);
    }

    public (int width, int height) CalculateTargetSize(int originalWidth, int originalHeight, float fontAspectRatio)
    {
        if (originalWidth == 0 || originalHeight == 0)
            throw new ArgumentException("Нулевой размер исходника.");

        double imageRatio = originalWidth / (double)originalHeight;
        int targetWidth = AsciiWidth;
        int targetHeight = (int)((targetWidth / imageRatio) * fontAspectRatio);
        if (targetHeight <= 0) targetHeight = 1;
        return (targetWidth, targetHeight);
    }
}