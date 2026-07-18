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

        var resized = new SKBitmap(targetWidth, targetHeight);
        
        using (var canvas = new SKCanvas(resized))
        using (var paint = new SKPaint())
        {
            paint.FilterQuality = SKFilterQuality.High;
            paint.IsAntialias = true;
            
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(bitmap, 
                new SKRect(0, 0, targetWidth, targetHeight), 
                paint);
        }
        
        return resized;
    }
}