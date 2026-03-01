namespace ImageToASCII.Core.Models;

public sealed record AsciiPalette(
    string Name,
    IReadOnlyList<char> Characters,
    string Description
);

public static class AsciiPaletteRegistry
{
    public static AsciiPalette Basic { get; } = new(
        "Basic",
        [' ', '.', ':', '-', '=', '+', '*', '#', '%', '@'],
        "Универсальная"
    );

    public static AsciiPalette Detailed { get; } = new(
        "Detailed",
        [' ', '.', '`', ',', ':', ';', '~', '+', 'i', '>', 'r', 'c', 'v', 'u', 'n', 'z', 
         'X', '0', 'O', 'Q', 'm', 'W', '&', '8', '%', 'B', '@'],
        "Сбалансированная (четкий градиент)"
    );

    public static AsciiPalette Geometric { get; } = new(
        "Geometric",
        [' ', '·', '•', 'o', 'O', '0', 'm', 'M', '@', '█'],
        "Геометрическая"
    );

    public static AsciiPalette Blocks { get; } = new(
        "Blocks",
        [' ', '\u2591', '\u2592', '\u2593', '\u2588'], // ░▒▓█
        "Блочная (мягкие переходы)"
    );

    public static AsciiPalette Minimal { get; } = new(
        "Minimal",
        [' ', '.', ':', '!', 'i', '|', 'H', 'M', '@'],
        "Минималистичная"
    );

    public static AsciiPalette Letters { get; } = new(
        "Letters",
        [' ', 'i', 't', 'r', 'o', 'c', 'a', 'b', 'p', 'w', 'k', 'N', 'Q', 'M', '@'],
        "Буквенная"
    );

    public static AsciiPalette Lines { get; } = new(
        "Lines",
        [' ', '-', '=', '≡', '*', '#', '%', '@'],
        "Линейная"
    );

    public static AsciiPalette Dense { get; } = new(
        "Dense",
        [' ', '.', ':', '*', 'o', '&', '8', '#', '@', '█'],
        "Плотная"
    );

    public static AsciiPalette Retro { get; } = new(
        "Retro",
        [' ', '.', ':', '-', '=', '+', '*', 'x', 'X', '#', '%', '@'],
        "Ретро"
    );

    public static AsciiPalette AsciiArt { get; } = new(
        "AsciiArt",
        [' ', '.', ',', ':', ';', '+', '*', '?', '%', 'S', '#', '@'],
        "Классический ASCII"
    );

    public static IReadOnlyList<AsciiPalette> All { get; } = 
    [
        Basic,
        Detailed,
        Geometric,
        Blocks,
        Minimal,
        Letters,
        Lines,
        Dense,
        Retro,
        AsciiArt
    ];
}