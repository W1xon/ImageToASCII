using System.Collections.Generic;
using System.Drawing;
using ImageToASCII.ColorClassifiers;

namespace ImageToASCII.Extensions;

public static class BitmapExtensions
{
    public static readonly List<ConsoleColor> ConsoleColors = new();
    public static readonly List<Color> Colors = new();

    public static void ToGrayscale(this Bitmap bitmap, IColorClassifier colorClassifier)
    {
        ConsoleColors.Clear();
        Colors.Clear();

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);

                ConsoleColors.Add(colorClassifier.GetConsoleColor(pixel.R, pixel.G, pixel.B));
                Colors.Add(colorClassifier.GetColor(pixel.R, pixel.G, pixel.B));

                int gray = (pixel.R + pixel.G + pixel.B) / 3;
                bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, gray, gray, gray));
            }
        }
    }
}