using SkiaSharp;

namespace ImageToASCII.ColorSystem;

public interface IColorClassifier
{ 
    uint GetColor(byte r, byte g, byte b);
}