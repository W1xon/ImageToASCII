namespace ImageToASCII.ColorSystem;

public class NaturalPaletteClassifier : IColorClassifier
{
    public uint GetColor(byte red, byte green, byte blue)
        => ColorUtils.Pack(red, green, blue, 255);
}