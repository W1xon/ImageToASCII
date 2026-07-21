using SkiaSharp;

namespace ImageToASCII.ColorSystem;
public sealed class Palette7ColorClassifier : PaletteColorClassifierBase
{
    protected override SKColor[] Palette
        => ColorPaletteRegistry.Basic7;
}