using SkiaSharp;

namespace ImageToASCII.Core.Converters;

public sealed class BitmapToAsciiConverter
{
    private readonly char[] _asciiTable;
    private char[,]? _buffer;

    public BitmapToAsciiConverter(char[] asciiTable)
    {
        if (asciiTable == null || asciiTable.Length == 0)
            throw new ArgumentException("ASCII table must not be empty.", nameof(asciiTable));

        _asciiTable = asciiTable;
    }

    public char[,] Convert(SKBitmap bitmap)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        int height = bitmap.Height;
        int width = bitmap.Width;

        if (_buffer == null ||
            _buffer.GetLength(0) != height ||
            _buffer.GetLength(1) != width)
        {
            _buffer = new char[height, width];
        }

        ReadOnlySpan<byte> pixels = bitmap.GetPixelSpan();
        int lastIndex = _asciiTable.Length - 1;
        int rowBytes = bitmap.RowBytes;

        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * rowBytes;

            for (int x = 0; x < width; x++)
            {
                int pixelOffset = rowOffset + (x << 2);

                byte r = pixels[pixelOffset + 0];
                byte g = pixels[pixelOffset + 1];
                byte b = pixels[pixelOffset + 2];

                int luminance = (r * 77 + g * 150 + b * 29) >> 8;
                int index = (luminance * lastIndex) / 255;

                _buffer[y, x] = _asciiTable[index];
            }
        }

        return _buffer;
    }
}