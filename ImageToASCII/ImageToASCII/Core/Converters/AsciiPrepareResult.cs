using SkiaSharp;

namespace ImageToASCII.Core.Converters;

public class AsciiPrepareResult
{
    public SKBitmap ResizedBitmap { get; set; }
    public char[,] AsciiChars { get; set; }
    public int OutputWidth { get; set; }
    public int OutputHeight { get; set; }

    public uint[] Colors { get; set; }
}