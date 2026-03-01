using System.Runtime.CompilerServices;
using SkiaSharp;

namespace ImageToASCII.ColorSystem;

public static class ColorUtils
{
    /// <summary>
    /// Packs color components into ARGB (0xAARRGGBB) format.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Pack(byte red, byte green, byte blue, byte alpha = 255)
    {
        return ((uint)alpha << 24)
               | ((uint)red   << 16)
               | ((uint)green << 8)
               | blue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Pack(SKColor color)
        => Pack(color.Red, color.Green, color.Blue, color.Alpha);
}