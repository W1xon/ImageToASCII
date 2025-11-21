using System.Drawing;

namespace ImageToASCII.ColorClassifiers;

public interface IColorClassifier
{
     ConsoleColor GetConsoleColor(byte r, byte g, byte b);
     Color GetColor(byte r, byte g, byte b);
}