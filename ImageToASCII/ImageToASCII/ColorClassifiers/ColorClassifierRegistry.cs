namespace ImageToASCII.ColorClassifiers;

public static class ColorClassifierRegistry
{
    private static readonly Dictionary<int, IColorClassifier> _classifiersByIndex = new()
    {
        { 1, new BasicColorClassifier() },
        { 2, new Palette16ColorClassifier() },
        { 3, new Palette7ColorClassifier() },
        { 4, new NaturalPaletteClassifier() }
    };

    public static IColorClassifier GetClassifier(int index)
    {
        if (_classifiersByIndex.TryGetValue(index, out var classifier))
            return classifier;

        throw new KeyNotFoundException($"Color classifier with index {index} does not exist.");
    }
}