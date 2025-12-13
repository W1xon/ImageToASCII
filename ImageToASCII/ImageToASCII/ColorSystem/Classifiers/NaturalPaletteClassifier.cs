namespace ImageToASCII.ColorSystem;

public class NaturalPaletteClassifier : IColorClassifier
{
    public uint GetColor(byte r, byte g, byte b)
    {
        return ColorUtils.Pack(r, g, b, 255);
    }
}