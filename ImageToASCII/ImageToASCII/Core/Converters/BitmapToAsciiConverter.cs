using SkiaSharp;

namespace ImageToASCII.Core.Converters;

public class BitmapToAsciiConverter
{
    private char[] _asciiTable;
    private char[,] asciiResult;
    
    
    public BitmapToAsciiConverter(char[] asciiTable)
    {
        _asciiTable = asciiTable;
    }
    
    public char[,] Convert(SKBitmap bitmap)
    {
        int height = bitmap.Height;
        int width = bitmap.Width;
        
        if (asciiResult == null || asciiResult.GetLength(0) != height || asciiResult.GetLength(1) != width)
            asciiResult = new char[height, width];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                
                double brightness = (0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue) / 255.0;
                
                int mapIndex = (int)(brightness * (_asciiTable.Length - 1));
                
                if (mapIndex < 0) mapIndex = 0;
                if (mapIndex >= _asciiTable.Length) mapIndex = _asciiTable.Length - 1;
                
                asciiResult[y, x] = _asciiTable[mapIndex];
            }
        }
        
        return asciiResult;
    }
    
}