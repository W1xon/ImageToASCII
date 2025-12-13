using System.Runtime.CompilerServices;
using SkiaSharp;

namespace ImageToASCII.ColorSystem;
public static class ColorUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Pack(byte r, byte g, byte b, byte a = 255)
        => (uint)((uint)a << 24 | (uint)r << 16 | (uint)g << 8 | b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Pack(SKColor c) => Pack(c.Red, c.Green, c.Blue, c.Alpha);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetA(uint packed) => (byte)(packed >> 24);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetR(uint packed) => (byte)(packed >> 16);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetG(uint packed) => (byte)(packed >> 8);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetB(uint packed) => (byte)packed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SKColor ToSKColor(uint packed) =>
        new SKColor(GetR(packed), GetG(packed), GetB(packed), GetA(packed));
}