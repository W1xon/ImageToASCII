using SkiaSharp;

namespace ImageToASCII.ColorSystem;

public static class ColorPaletteRegistry
{
    public static readonly SKColor[] Basic16 =
    [
        new (0, 0, 0),
        new (0, 0, 128),
        new (0, 128, 0),
        new (0, 128, 128),
        new (128, 0, 0),
        new (128, 0, 128),
        new (128, 128, 0),
        new (192, 192, 192),
        new (128, 128, 128),
        new (0, 0, 255),
        new (0, 255, 0),
        new (0, 255, 255),
        new (255, 0, 0),
        new (255, 0, 255),
        new (255, 255, 0),
        new (255, 255, 255)
    ];

    public static readonly SKColor [] Basic7 =
    [
       new (255, 0, 0),
       new (0, 255, 0),
       new (0, 0, 255),
       new (255, 255, 0),
       new (255, 0, 255),
       new (0, 255, 255),
       new (255, 255, 255)
    ];
}