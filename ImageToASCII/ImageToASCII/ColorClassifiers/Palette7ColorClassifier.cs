using System.Drawing;

namespace ImageToASCII.ColorClassifiers;

public class Palette7ColorClassifier : IColorClassifier
{
   
    
    public ConsoleColor GetConsoleColor(byte r, byte g, byte b)
    {
        return ColorPaletteRegistry.Basic7
            .OrderBy(c => Distance(r, g, b, c.rgb.R, c.rgb.G, c.rgb.B))
            .First()
            .color;
    }

    public Color GetColor(byte r, byte g, byte b)
    {
        return ColorPaletteRegistry.Basic7
            .OrderBy(c => Distance(r, g, b, c.rgb.R, c.rgb.G, c.rgb.B))
            .First()
            .rgb;
    }


    private double Distance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
    {
        int dr = r1 - r2;
        int dg = g1 - g2;
        int db = b1 - b2;
        return dr * dr + dg * dg + db * db;
    }
}