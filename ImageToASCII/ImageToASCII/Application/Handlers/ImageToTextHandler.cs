using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Processors;
using ImageToASCII.Services;
using ImageToASCII.UI;
using SkiaSharp;

namespace ImageToASCII.Application;

public sealed class ImageToTextHandler : AsciiHandlerBase
{
    private const string ImageFilter =
        "Изображения|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.webp|Все файлы|*.*";

    private const string DialogTitle = "Выберите изображение";

    private TextAsciiExporter _textExporter = null!;

    protected override string GetFileFilter() => ImageFilter;

    protected override string GetFileDialogTitle() => DialogTitle;

    protected override PaletteType AskPaletteType()
    {
        return 0; // текстовый режим без цветовой классификации
    }

    protected override bool CollectSettings()
    {
        base.CollectSettings();

        _textExporter = new TextAsciiExporter(AsciiConverter)
        {
            AsciiWidth = Settings.Width
        };

        return true;
    }

    protected override async Task ExecuteConversionAsync()
    {
        using var sourceBitmap = SKBitmap.Decode(Settings.InputFilePath);

        var outputPath = new OutputNameBuilder(Settings)
            .BuildText();

        ConsoleUI.ShowProgress("Сохранение в текстовый файл...");

        _textExporter.SaveToFile(sourceBitmap, outputPath);

        ConsoleUI.ShowFileResult(outputPath);

        await Task.CompletedTask;
    }
}