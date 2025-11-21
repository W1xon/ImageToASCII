using System.Drawing;

namespace ImageToASCII.ColorClassifiers;

public class BasicColorClassifier : IColorClassifier
{
    private const byte _highThreshold = 200;
    private const byte _midThreshold  = 150;
    private const byte _lowThreshold  = 50;
    private const byte _grayThreshold = 100;
    
    // Конвертация RGB в ближайший цвет консоли
    public ConsoleColor GetConsoleColor(byte red, byte green, byte blue)
    {

        if (red > _highThreshold && green > _highThreshold && blue > _highThreshold) return ConsoleColor.White;
        if (red < _lowThreshold && green < _lowThreshold && blue < _lowThreshold) return ConsoleColor.Black;
        if (red > _highThreshold && green < _lowThreshold && blue < _lowThreshold) return ConsoleColor.Red;
        if (red < _lowThreshold && green > _highThreshold && blue < _lowThreshold) return ConsoleColor.Green;
        if (red < _lowThreshold && green < _lowThreshold && blue > _highThreshold) return ConsoleColor.Blue;
        if (red > _highThreshold && green > _highThreshold && blue < _lowThreshold) return ConsoleColor.Yellow;
        if (red > _highThreshold && green < _lowThreshold && blue > _highThreshold) return ConsoleColor.Magenta;
        if (red < _lowThreshold && green > _highThreshold && blue > _highThreshold) return ConsoleColor.Cyan;

        if (red > _midThreshold && green > _midThreshold && blue > _midThreshold) return ConsoleColor.Gray;
        if (red > _grayThreshold && green > _grayThreshold && blue > _grayThreshold) return ConsoleColor.DarkGray;

        if (red > green && red > blue) return ConsoleColor.DarkRed;
        if (green > red && green > blue) return ConsoleColor.DarkGreen;
        if (blue > red && blue > green) return ConsoleColor.DarkBlue;

        if (red > green && red > blue && green > blue) return ConsoleColor.DarkYellow;
        if (red > green && red > blue && blue > green) return ConsoleColor.DarkMagenta;

        return ConsoleColor.Gray;
    }

    public Color GetColor(byte red, byte green, byte blue)
    {
        if (red > _highThreshold && green > _highThreshold && blue > _highThreshold) return Color.White;
        if (red < _lowThreshold && green < _lowThreshold && blue < _lowThreshold) return Color.Black;
        if (red > _highThreshold && green < _lowThreshold && blue < _lowThreshold) return Color.Red;
        if (red < _lowThreshold && green > _highThreshold && blue < _lowThreshold) return Color.Green;
        if (red < _lowThreshold && green < _lowThreshold && blue > _highThreshold) return Color.Blue;
        if (red > _highThreshold && green > _highThreshold && blue < _lowThreshold) return Color.Yellow;
        if (red > _highThreshold && green < _lowThreshold && blue > _highThreshold) return Color.Magenta;
        if (red < _lowThreshold && green > _highThreshold && blue > _highThreshold) return Color.Cyan;

        if (red > _midThreshold && green > _midThreshold && blue > _midThreshold) return Color.LightGray;
        if (red > _grayThreshold && green > _grayThreshold && blue > _grayThreshold) return Color.Gray;

        if (red > green && red > blue) return Color.DarkRed;
        if (green > red && green > blue) return Color.DarkGreen;
        if (blue > red && blue > green) return Color.DarkBlue;

        if (red > green && red > blue && green > blue) return Color.Goldenrod;
        if (red > green && red > blue && blue > green) return Color.DeepPink;

        return Color.Gray;
    }
}
