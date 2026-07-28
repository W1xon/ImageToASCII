using System.Text;
using ImageToASCII.Core.Converters;
using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public class TextAsciiExporter : ImageProcessorBase
{
    public int MaxLineLength { get; set; } = 0;
    private IReporter _reporter;

    public TextAsciiExporter(BitmapToAsciiConverter converter, IReporter reporter) : base(converter)
    {
        _reporter = reporter;
    }

    public void SaveToFile(SKBitmap bitmap, string outputPath, bool showUi = true)
    {
        if (showUi) _reporter.ShowInfo("Генерация ASCII (txt)...");
        string text = GetAsciiText(bitmap);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        File.WriteAllText(outputPath, text, Encoding.UTF8);
        if (showUi) _reporter.ShowSuccess($"Сохранён TXT: {Path.GetFileName(outputPath)}");
    }

    private string GetAsciiText(SKBitmap bitmap)
    {
        char[] grid = _asciiConverter.Convert(bitmap, out int width, out int height);
        var sb = new StringBuilder(height * (width + 1));
        for (int y = 0; y < height; y++)
        {
            int lineLength = 0;
            int rowBaseIdx = y * width;
            for (int x = 0; x < width; x++)
            {
                sb.Append(grid[rowBaseIdx + x]);
                lineLength++;
                if (MaxLineLength > 0 && lineLength >= MaxLineLength)
                {
                    sb.Append('\n');
                    lineLength = 0;
                }
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}