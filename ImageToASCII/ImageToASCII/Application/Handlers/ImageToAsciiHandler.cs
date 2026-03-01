using ImageToASCII.ColorSystem;
using ImageToASCII.Services;
using ImageToASCII.UI;
using SkiaSharp;

namespace ImageToASCII.Application;

public sealed class ImageToAsciiHandler : AsciiHandlerBase
{
    private const string ImageFilter =
        "Изображения|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.webp|Все файлы|*.*";

    private const string DialogTitle = "Выберите изображение";

    protected override string GetFileFilter() => ImageFilter;

    protected override string GetFileDialogTitle() => DialogTitle;

    protected override async Task ExecuteConversionAsync()
    {
        using var sourceBitmap = SKBitmap.Decode(Settings.InputFilePath);

        var classifier = ColorClassifierFactory
            .Create(Settings.PaletteType);

        var outputPath = new OutputNameBuilder(Settings)
            .BuildImage();

        ConsoleUI.ShowProgress("Генерация ASCII изображения...");

        using var asciiBitmap =
            AsciiExporter.PrintAndSave(sourceBitmap, classifier);

        using var image  = SKImage.FromBitmap(asciiBitmap);
        using var data   = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);

        data.SaveTo(stream);

        ConsoleUI.ShowFileResult(outputPath);

        await Task.CompletedTask;
    }
}