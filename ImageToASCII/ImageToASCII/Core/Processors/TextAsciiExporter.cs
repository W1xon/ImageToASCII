using System.IO;
using System.Text;
using ImageToASCII.Core.Converters;
using ImageToASCII.UI;
using SkiaSharp;

namespace ImageToASCII.Core.Processors;

public class TextAsciiExporter : ImageProcessorBase
{
    public int MaxLineLength { get; set; } = 0;

    public TextAsciiExporter(BitmapToAsciiConverter converter) : base(converter)
    {
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
        char[,] grid = _asciiConverter.Convert(bitmap);

        int height = grid.GetLength(0);
        int width = grid.GetLength(1);

        var sb = new StringBuilder(height * (width + 1));

        for (int y = 0; y < height; y++)
        {
            int lineLength = 0;

            for (int x = 0; x < width; x++)
            {
                sb.Append(grid[y, x]);
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