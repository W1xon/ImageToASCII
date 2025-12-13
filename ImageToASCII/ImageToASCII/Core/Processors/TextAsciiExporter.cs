using System.IO;
using System.Text;
using ImageToASCII.Core.Converters;
using ImageToASCII.UI;
using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public class TextAsciiExporter  : ImageProcessorBase
{
    public int MaxLineLength { get; set; } = 0;

    public TextAsciiExporter(BitmapToAsciiConverter converter) : base(converter)
    {
        _asciiConverter = converter;
    }


    public void SaveToFile(SKBitmap bitmap, string outputPath, bool showUi = true)
    {
        if (showUi) ConsoleUI.ShowProgress("Генерация ASCII (txt)...");
        string text = GetAsciiText(bitmap);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        File.WriteAllText(outputPath, text, Encoding.UTF8);
        if (showUi) ConsoleUI.WriteSuccess($"Сохранён TXT: {Path.GetFileName(outputPath)}");
    }

    private string GetAsciiText(SKBitmap bitmap)
    {
        var resized = ResizeBitmap(bitmap, true);
        var grid = _asciiConverter.Convert(resized);
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        var sb = new StringBuilder(rows * (cols + 2));

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
                sb.Append(grid[y, x]);

            sb.AppendLine();
        }

        return sb.ToString(); 
    }
}
