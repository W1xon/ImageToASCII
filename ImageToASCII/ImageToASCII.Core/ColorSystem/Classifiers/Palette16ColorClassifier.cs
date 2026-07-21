using SkiaSharp;
namespace ImageToASCII.ColorSystem;

public sealed class Palette16ColorClassifier : PaletteColorClassifierBase
{
    protected override SKColor[] Palette
        => ColorPaletteRegistry.Basic16;
}