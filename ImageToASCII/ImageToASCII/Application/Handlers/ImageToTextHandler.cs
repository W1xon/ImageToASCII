using ImageToASCII.Core.Converters;
using ImageToASCII.Core.Processors;
using ImageToASCII.Services;
using ImageToASCII.UI;
using SkiaSharp;

namespace ImageToASCII.Application;


public class ImageToTextHandler : BaseHandler
{
    private BitmapToAsciiConverter _converter;
    private TextAsciiExporter _exporter;
    
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
        Settings.PaletteType = 0;
        
        _converter = new BitmapToAsciiConverter(Settings.AsciiPalette);
        _exporter = new TextAsciiExporter(_converter);
        _exporter.AsciiWidth = Settings.Width;
        
        return true;
    }
    
    protected override async Task ExecuteConversionAsync()
    {
        using var bitmap = SKBitmap.Decode(Settings.InputFilePath);
        
        var nameBuilder = new OutputNameBuilder(Settings);
        string outputPath = nameBuilder.BuildText();
        
        ConsoleUI.ShowProgress("Сохранение в текстовый файл...");
        
        _exporter.SaveToFile(bitmap, outputPath);
        
        ConsoleUI.ShowFileResult(outputPath);
        
        await Task.CompletedTask;
    }
}