using SkiaSharp;
namespace ImageToASCII.ColorSystem;

public class Palette16ColorClassifier : IColorClassifier
{
    public uint GetColor(byte r, byte g, byte b)
    {
        var best = default(SKColor);
        double bestDist = double.MaxValue;

        foreach (var c in ColorPaletteRegistry.Basic16)
        {
            double d = PerceptualDistance(r, g, b, c.Red, c.Green, c.Blue);
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }

        return ColorUtils.Pack(best);
    }

    private static double PerceptualDistance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
    {
        const double redWeight = 2.0;
        const double greenWeight = 4.0;
        const double blueWeight = 3.0;

        int dr = r1 - r2;
        int dg = g1 - g2;
        int db = b1 - b2;

        return redWeight * dr * dr + greenWeight * dg * dg + blueWeight * db * db;
    }
}