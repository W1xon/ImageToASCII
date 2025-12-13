namespace ImageToASCII.Core.Models;

public static class ASCIIPalette
{
    public static readonly char[] Basic =
        [' ', '.', ':', '-', '=', '+', '*', '#', '%', '@'];

    public static readonly char[] Detailed =
        [
            ' ', '.', '\'', ',', ':', ';', '-', '~', '=', '+', 
            'i', 'l', 'I', '!', '/', '|', '(', ')', '1', 'c', 
            'v', 'x', 'z', 'L', 'C', 'J', 'U', 'Y', 'X', 'Z', 
            'O', '0', 'Q', '#', 'M', 'W', '&', '8', '%', 'B', '@'
        ];

    public static readonly char[] Geometric =
        [' ', '.', ',', 'o', 'O', '0', '@', '#', '&', 'B'];

    public static readonly char[] Blocks =
        [' ', '.', '\u2591', ':', '\u2592', '+', '\u2593', '#', '%', '\u2588']; // ░▒▓█

    public static readonly char[] Minimal =
        [' ', '.', ',', ':', 'o', '*', 'O', '0', '@', '#'];

    public static readonly char[] AsciiArt =
        [' ', '.', ',', ':', ';', '+', '*', '?', '%', 'S', '#', '@'];

    public static readonly char[] Letters =
        [
            ' ', 'i', 'l', 't', 'f', 'j', 'r', 'x', 'n', 'u', 
            'v', 'c', 'z', 'X', 'Y', 'U', 'J', 'C', 'L', 'Q', 
            'O', '0', 'Z', 'M', 'W', 'N', 'B', '@'
        ];

    public static readonly char[] Lines =
        [' ', '.', '-', '=', '_', '+', '#', '%', '@', '&'];

    public static readonly char[] Dense =
        [' ', '.', ',', ':', ';', 'i', 'l', 't', 'f', 'L', 'F', 'E', 'H', '#', '%', '@'];

    public static readonly char[] Retro =
        [' ', '.', ':', '-', '=', '+', '*', 'x', 'X', '#', '%', '@'];

    public static List<(string Name, char[] Palette, string Description)> GetAllWithDescriptions() =>
    [
        ("Basic", Basic, "Универсальная"),
        ("Geometric", Geometric, "Геометрия"),
        ("Detailed", Detailed, "Детальная"),
        ("Blocks", Blocks, "Блоки"),
        ("Minimal", Minimal, "Минимализм"),
        ("AsciiArt", AsciiArt, "ASCII Art"),
        ("Letters", Letters, "Буквенная"),
        ("Lines", Lines, "Штриховка"),
        ("Dense", Dense, "Плотная"),
        ("Retro", Retro, "Ретро")
    ];
}