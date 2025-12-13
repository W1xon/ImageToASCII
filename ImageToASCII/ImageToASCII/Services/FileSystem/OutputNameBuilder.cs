using ImageToASCII.Core.Models;

namespace ImageToASCII.Services;

public class OutputNameBuilder
{
    private readonly ConversionSettings _settings;
    private readonly string _originalFileName;

    public OutputNameBuilder(ConversionSettings settings)
    {
        _settings = settings;
        _originalFileName = Path.GetFileNameWithoutExtension(_settings.InputFilePath);
    }
    public string Build(string extension)
    {
        string fileName = $"{BuildBaseName()}{extension}";
        return Path.Combine(_settings.OutputDirectory, fileName);
    }

    public string BuildImage() => Build(".png");

    public string BuildText() => Build(".txt");

    public string BuildVideo() => Build(".mp4");

    public string GetFileName(string extension) => $"{BuildBaseName()}{extension}";

    private string BuildBaseName()
    {
        return $"{_originalFileName}_{GetAsciiPaletteName()}_{GetColorMethodName()}_{_settings.Width}px";
    }

    private string GetAsciiPaletteName()
    {
        if (_settings.AsciiPalette == null || _settings.AsciiPalette.Length == 0)
            return "unknown";

        var palettes = new Dictionary<char[], string>
        {
            { ASCIIPalette.Basic, "basic" },
            { ASCIIPalette.Geometric, "geometric" },
            { ASCIIPalette.Detailed, "detailed" },
            { ASCIIPalette.Blocks, "blocks" },
            { ASCIIPalette.Lines, "lines" },
            { ASCIIPalette.Minimal, "minimal" },
            { ASCIIPalette.AsciiArt, "asciiart" },
            { ASCIIPalette.Dense, "technical" },
            { ASCIIPalette.Letters, "letters" },
            { ASCIIPalette.Retro, "artistic" }
        };

        foreach (var (palette, name) in palettes)
        {
            if (_settings.AsciiPalette.SequenceEqual(palette))
                return name;
        }

        return "custom";
    }
    private string GetColorMethodName()
    {
        return _settings.PaletteType switch
        {
            1 => "grayscale",
            2 => "color16",
            3 => "color7",
            4 => "natural",
            5 => "vibrant",
            6 => "pastel",
            7 => "neon",
            8 => "monochrome",
            _ => "default"
        };
    }
}