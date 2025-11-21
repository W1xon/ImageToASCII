using System.Drawing;

namespace ImageToASCII.ColorClassifiers;

public class ColorPaletteRegistry
{
    public static readonly (ConsoleColor color, Color rgb)[] Basic16 =
    [
        (ConsoleColor.Black,      Color.FromArgb(0,0,0)),
        (ConsoleColor.DarkBlue,   Color.FromArgb(0,0,128)),
        (ConsoleColor.DarkGreen,  Color.FromArgb(0,128,0)),
        (ConsoleColor.DarkCyan,   Color.FromArgb(0,128,128)),
        (ConsoleColor.DarkRed,    Color.FromArgb(128,0,0)),
        (ConsoleColor.DarkMagenta,Color.FromArgb(128,0,128)),
        (ConsoleColor.DarkYellow, Color.FromArgb(128,128,0)),
        (ConsoleColor.Gray,       Color.FromArgb(192,192,192)),
        (ConsoleColor.DarkGray,   Color.FromArgb(128,128,128)),
        (ConsoleColor.Blue,       Color.FromArgb(0,0,255)),
        (ConsoleColor.Green,      Color.FromArgb(0,255,0)),
        (ConsoleColor.Cyan,       Color.FromArgb(0,255,255)),
        (ConsoleColor.Red,        Color.FromArgb(255,0,0)),
        (ConsoleColor.Magenta,    Color.FromArgb(255,0,255)),
        (ConsoleColor.Yellow,     Color.FromArgb(255,255,0)),
        (ConsoleColor.White,      Color.FromArgb(255,255,255))
    ];

    // Минимальная палитра 7 основных цветов
    public static readonly (ConsoleColor color, Color rgb)[] Basic7 =
    [
        (ConsoleColor.Red,     Color.FromArgb(255,0,0)),
        (ConsoleColor.Green,   Color.FromArgb(0,255,0)),
        (ConsoleColor.Blue,    Color.FromArgb(0,0,255)),
        (ConsoleColor.Yellow,  Color.FromArgb(255,255,0)),
        (ConsoleColor.Magenta, Color.FromArgb(255,0,255)),
        (ConsoleColor.Cyan,    Color.FromArgb(0,255,255)),
        (ConsoleColor.White,   Color.FromArgb(255,255,255))
    ];
}