namespace ImageToASCII.Core.Models;

public class ConversionSettings
{
    public string InputFilePath { get; set; }
    public int PaletteType { get; set; }
    public int Width { get; set; }
    public char[] AsciiPalette { get; set; }
    public string OutputDirectory { get; set; }
    
    public ConversionSettings()
    {
        OutputDirectory = Path.Combine(AppContext.BaseDirectory, "Output");
        Directory.CreateDirectory(OutputDirectory);
    }
}