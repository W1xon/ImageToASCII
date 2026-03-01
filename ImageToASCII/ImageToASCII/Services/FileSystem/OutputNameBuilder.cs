using ImageToASCII.Core.Models;

namespace ImageToASCII.Services;

public sealed class OutputNameBuilder
{
    private readonly ConversionSettings _settings;
    private readonly string _originalFileName;

    public OutputNameBuilder(ConversionSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _originalFileName = Path.GetFileNameWithoutExtension(_settings.InputFilePath);
    }

    public string Build(string extension)
    {
        string fileName = $"{BuildBaseName()}{extension}";
        return Path.Combine(_settings.OutputDirectory, fileName);
    }

    public string BuildImage() => Build(".png");
    public string BuildText()  => Build(".txt");
    public string BuildVideo() => Build(".mp4");

    public string GetFileName(string extension)
        => $"{BuildBaseName()}{extension}";

    private string BuildBaseName()
    {
        return $"{_originalFileName}_{GetAsciiPaletteName()}_{GetColorMethodName()}_{_settings.Width}px";
    }

    private string GetAsciiPaletteName()
        => _settings.AsciiPalette?.Name.ToLowerInvariant() ?? "unknown";

    private string GetColorMethodName()
        => _settings.PaletteType.ToString();
}