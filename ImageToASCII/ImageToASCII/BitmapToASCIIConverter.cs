using System.Drawing;

namespace ImageToASCII;

public class BitmapToASCIIConverter
{
    private readonly char[] _asciiTable =
    [
        ' ', '.', ',', ':', ';', '-', '~', '*', '+', '=',
        'o', 'O', '0', 'Q', 'C', '8', '#', '@'
    ];

    public char[,] Convert(Bitmap bitmap)
    {
        int height = bitmap.Height;
        int width = bitmap.Width;
        var asciiResult = new char[height, width];
        int maxIndex = _asciiTable.Length - 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                
                int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                
                int tableIndex = (brightness * maxIndex + 127) / 255;
                
                tableIndex = Math.Clamp(tableIndex, 0, maxIndex);
                
                asciiResult[y, x] = _asciiTable[tableIndex];
            }
        }

        return asciiResult;
    }
}