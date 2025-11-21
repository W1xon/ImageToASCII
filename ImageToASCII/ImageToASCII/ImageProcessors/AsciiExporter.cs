using System.Drawing;
using ImageToASCII.ColorClassifiers;
using ImageToASCII.Extensions;

namespace ImageToASCII.ImageProcessors;

public class AsciiExporter : ImageProcessorBase
{
    public AsciiExporter(BitmapToASCIIConverter converter) : base(converter)
    {
    }

    public Bitmap PrintAndSave(Bitmap bitmap, IColorClassifier colorClassifier)
    {
        int fontSize = 16;
        var resizedBitmap = ResizeBitmap(bitmap, false);
        var asciiChars = _asciiConverter.Convert(resizedBitmap);

        resizedBitmap.ToGrayscale(colorClassifier);

        var outputBitmap = new Bitmap(resizedBitmap.Width * fontSize,
            resizedBitmap.Height * fontSize);

        using Graphics graphics = Graphics.FromImage(outputBitmap);
        graphics.Clear(Color.Black);

        using Font font = new Font("Arial", fontSize);
        int colorIndex = 0;

        for (int y = 0; y < asciiChars.GetLength(0); y++)
        {
            for (int x = 0; x < asciiChars.GetLength(1); x++)
            {
                string symbol = asciiChars[y, x].ToString();
                using Brush brush = new SolidBrush(BitmapExtensions.Colors[colorIndex]);
                graphics.DrawString(symbol, font, brush, 
                    x * fontSize, y * fontSize, 
                    StringFormat.GenericTypographic);
                colorIndex++;
            }
        }

        return outputBitmap;
    }
}