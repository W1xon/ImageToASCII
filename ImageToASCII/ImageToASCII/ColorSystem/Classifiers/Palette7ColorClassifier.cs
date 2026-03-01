using SkiaSharp;

namespace ImageToASCII.ColorSystem;
public sealed class Palette7ColorClassifier : PaletteColorClassifierBase
{
    protected override IReadOnlyList<SKColor> Palette
        => ColorPaletteRegistry.Basic7;
}