using SkiaSharp;

namespace ImageToASCII.ColorSystem;

public abstract class PaletteColorClassifierBase : IColorClassifier
{
    private const double RedWeight = 2.0;
    private const double GreenWeight = 4.0;
    private const double BlueWeight = 3.0;
    protected abstract IReadOnlyList<SKColor> Palette { get; }

    public uint GetColor(byte red, byte green, byte blue)
    {
        SKColor closestColor = default;
        double minDistance = double.MaxValue;

        foreach (var paletteColor in Palette)
        {
            var distance = CalculateDistance(
                red, green, blue,
                paletteColor.Red,
                paletteColor.Green,
                paletteColor.Blue);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = paletteColor;
            }
        }

        return ColorUtils.Pack(closestColor);
    }

    private static double CalculateDistance(
        byte r1, byte g1, byte b1,
        byte r2, byte g2, byte b2)
    {
        int dr = r1 - r2;
        int dg = g1 - g2;
        int db = b1 - b2;

        return RedWeight * dr * dr +
               GreenWeight * dg * dg +
               BlueWeight * db * db;
    }
}