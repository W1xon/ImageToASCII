using ImageToASCII.ColorSystem;

namespace ImageToASCII.Core.Models;
public class ConversionSettings
{
    public string InputFilePath { get; set; } = "";
    public PaletteType PaletteType { get; set; } = PaletteType.Basic;
    public int Width { get; set; } = 80;
    public AsciiPalette AsciiPalette { get; set; } = AsciiPaletteRegistry.Basic; 
    public string OutputDirectory { get; set; } =
        Path.Combine(AppContext.BaseDirectory, "Output");

    public ConversionSettings()
    {
        Directory.CreateDirectory(OutputDirectory);
    }
}