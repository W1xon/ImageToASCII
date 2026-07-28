namespace ImageToASCII.Core.Converters;

public class AsciiPrepareResult
{
    public char[] AsciiChars { get; set; } = null!;
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public int OutputWidth { get; set; }
    public int OutputHeight { get; set; }
    public uint[] Colors { get; set; } = null!;
}