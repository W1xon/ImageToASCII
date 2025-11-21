using System.Drawing;

namespace ImageToASCII.ColorClassifiers;

public class NaturalPaletteClassifier : IColorClassifier
{
    public ConsoleColor GetConsoleColor(byte r, byte g, byte b) => new Palette16ColorClassifier().GetConsoleColor(r, g, b);

    public Color GetColor(byte r, byte g, byte b) =>   Color.FromArgb(255,r,g,b);
}