using SkiaSharp;
using ImageToASCII.Core.Models;

namespace ImageToASCII.Core.Converters;

public class AsciiPrepareResult
{
    public SKBitmap ResizedBitmap { get; set; } = null!;
    public char[,] AsciiChars { get; set; } = null!;
    public int OutputWidth { get; set; }
    public int OutputHeight { get; set; }
    public uint[] Colors { get; set; } = null!;
}