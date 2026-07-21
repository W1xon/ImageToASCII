using SkiaSharp;

namespace ImageToASCII.ColorSystem;

public abstract class PaletteColorClassifierBase : IColorClassifier
{
    private const int RedWeight = 2;
    private const int GreenWeight = 4;
    private const int BlueWeight = 3;
    
    
    protected abstract SKColor[] Palette { get; }

    public uint GetColor(byte red, byte green, byte blue)
    {
        var palette = Palette.AsSpan();
        
        if (palette.IsEmpty) 
            return 0;

        SKColor closestColor = palette[0];
        int minDistance = int.MaxValue;

        int r1 = red;
        int g1 = green;
        int b1 = blue;

        for (int i = 0; i < palette.Length; i++)
        {
            ref readonly var paletteColor = ref palette[i];

            int dr = r1 - paletteColor.Red;
            int dg = g1 - paletteColor.Green;
            int db = b1 - paletteColor.Blue;

            int distance = RedWeight * dr * dr +
                           GreenWeight * dg * dg +
                           BlueWeight * db * db;

            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = paletteColor;
                
                if (distance == 0) 
                    break;
            }
        }

        return ColorUtils.Pack(closestColor);
    }
}