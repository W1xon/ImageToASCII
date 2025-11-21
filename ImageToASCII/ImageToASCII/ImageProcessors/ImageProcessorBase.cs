using System.Drawing;

namespace ImageToASCII.ImageProcessors;

public class ImageProcessorBase
{
    protected BitmapToASCIIConverter _asciiConverter;
    public const double WidthOffset = 1.8;
    public int AsciiWidth { get; set; } = 350;
    public ImageProcessorBase(BitmapToASCIIConverter converter)
    {
        _asciiConverter = converter;
    }


    protected Bitmap ResizeBitmap(Bitmap bitmap, bool applyWidthOffset)
    {
        double ratio = bitmap.Width / (double)bitmap.Height;

        int targetWidth = AsciiWidth;
        int targetHeight = applyWidthOffset 
            ? (int)(targetWidth / (ratio * WidthOffset)) 
            : (int)(targetWidth / ratio);

        return new Bitmap(bitmap, new Size(targetWidth, targetHeight));
    }
}