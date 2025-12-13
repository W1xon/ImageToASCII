using ImageToASCII.Core.Converters;
using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public class ImageProcessorBase
{
    protected BitmapToAsciiConverter _asciiConverter;
    private const double WidthOffset = 1.8;
    public int AsciiWidth { get; set; } = 350;

    public ImageProcessorBase(BitmapToAsciiConverter converter)
    {
        _asciiConverter = converter;
    }

    protected SKBitmap ResizeBitmap(SKBitmap bitmap, bool applyWidthOffset)
    {
        double ratio = bitmap.Width / (double)bitmap.Height;
        int targetWidth = AsciiWidth;
        int targetHeight = applyWidthOffset 
            ? (int)(targetWidth / (ratio * WidthOffset)) 
            : (int)(targetWidth / ratio);

        var resized = new SKBitmap(targetWidth, targetHeight);
        
        using (var canvas = new SKCanvas(resized))
        using (var paint = new SKPaint())
        {
            paint.FilterQuality = SKFilterQuality.Medium;
            paint.IsAntialias = true;
            
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(bitmap, 
                new SKRect(0, 0, targetWidth, targetHeight), 
                paint);
        }
        
        return resized;
    }
}