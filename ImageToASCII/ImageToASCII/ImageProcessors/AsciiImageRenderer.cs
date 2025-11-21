using System;
using System.Drawing;
using ImageToASCII.ColorClassifiers;
using ImageToASCII.Extensions;

namespace ImageToASCII.ImageProcessors;

public class AsciiImageRenderer : ImageProcessorBase
{
    public AsciiImageRenderer(BitmapToASCIIConverter converter) : base(converter)
    {
    }

    public void Print(Bitmap bitmap, IColorClassifier colorClassifier)
    {
        int colorIndex = 0;
        bitmap = ResizeBitmap(bitmap, true);
        bitmap.ToGrayscale(colorClassifier);

        var asciiRows = _asciiConverter.Convert(bitmap);
        Console.Clear();

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Console.ForegroundColor = BitmapExtensions.ConsoleColors[colorIndex];
                Console.Write(asciiRows[y, x]);
                colorIndex++;
            }
            Console.WriteLine();
        }

        Console.SetCursorPosition(0, 0);
    }
}