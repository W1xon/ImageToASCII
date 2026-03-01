namespace ImageToASCII.ColorSystem;
public enum PaletteType
{
    Basic = 1,
    Palette16 = 2,
    Palette7 = 3,
    Natural = 4
}

public static class ColorClassifierFactory
{
    public static IColorClassifier Create(PaletteType type)
    {
        return type switch
        {
            PaletteType.Basic      => new BasicColorClassifier(),
            PaletteType.Palette16  => new Palette16ColorClassifier(),
            PaletteType.Palette7   => new Palette7ColorClassifier(),
            PaletteType.Natural    => new NaturalPaletteClassifier(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported palette type.")
        };
    }
}