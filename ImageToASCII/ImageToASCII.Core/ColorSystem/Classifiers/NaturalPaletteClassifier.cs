namespace ImageToASCII.ColorSystem;

public sealed class NaturalPaletteClassifier : IColorClassifier
{
    public uint GetColor(byte red, byte green, byte blue)
        => ColorUtils.Pack(red, green, blue, 255);
}