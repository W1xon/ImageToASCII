namespace ImageToASCII.ColorSystem;

public static class ColorClassifierRegistry
{
    public static IColorClassifier GetClassifier(int paletteType)
    {
        return paletteType switch
        {
            1 => new BasicColorClassifier(),
            2 => new Palette16ColorClassifier(),
            3 => new Palette7ColorClassifier(),
            4 => new NaturalPaletteClassifier(),
            _ => throw new ArgumentException($"Неизвестный тип палитры: {paletteType}")
        };
    }
}