using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;
using ImageToASCII.Core.Processors;
using ImageToASCII.Services;
using ImageToASCII.UI;
using SkiaSharp;

namespace ImageToASCII.Application;

public class ImageToAsciiHandler : BaseHandler
{
    private BitmapToAsciiConverter _converter;
    private AsciiExporter _exporter;
    
    protected override string GetFileFilter()
    {
        return "Изображения|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.webp|Все файлы|*.*";
    }
    
    protected override string GetFileDialogTitle()
    {
        return "Выберите изображение";
    }
    
    protected override bool CollectSettings()
    {
        Settings.AsciiPalette = ConsoleUI.AskAsciiPalette();
        Settings.Width = ConsoleUI.AskWidth();
        Settings.PaletteType = ConsoleUI.AskPalette();
        
        _converter = new BitmapToAsciiConverter(Settings.AsciiPalette);
        _exporter = new AsciiExporter(_converter);
        _exporter.AsciiWidth = Settings.Width;
        
        return true;
    }
    
    protected override async Task ExecuteConversionAsync()
    {
        using var bitmap = SKBitmap.Decode(Settings.InputFilePath);
        
        var nameBuilder = new OutputNameBuilder(Settings);
        string outputPath = nameBuilder.BuildImage();
        
        ConsoleUI.ShowProgress($"Генерация ASCII изображения...");
        
        var classifier = ColorClassifierRegistry.GetClassifier(Settings.PaletteType);
        var asciiBitmap = _exporter.PrintAndSave(bitmap, classifier);
        
        using var image = SKImage.FromBitmap(asciiBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
        
        asciiBitmap.Dispose();
        
        ConsoleUI.ShowFileResult(outputPath);
        
        await Task.CompletedTask;
    }
}