using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;
using ImageToASCII.Core.Processors;
using ImageToASCII.Services;
using ImageToASCII.UI;

namespace ImageToASCII.Application;

public class VideoToAsciiHandler : BaseHandler
{
    private BitmapToAsciiConverter _converter;
    private AsciiExporter _exporter;
    
    protected override string GetFileFilter()
    {
        return "Видео|*.mp4;*.avi;*.mov;*.mkv|Все файлы|*.*";
    }
    
    protected override string GetFileDialogTitle()
    {
        return "Выберите видео";
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
        var classifier = ColorClassifierRegistry.GetClassifier(Settings.PaletteType);
        var videoConverter = new VideoToAsciiConverter(_exporter);
        
        await videoConverter.InitializeAsync();
        
        var nameBuilder = new OutputNameBuilder(Settings);
        string outputPath = nameBuilder.BuildVideo();
        
        ConsoleUI.ShowProgress("Начинаем конвертацию видео...");
        
        await videoConverter.Convert(Settings.InputFilePath, outputPath, classifier);
        
        ConsoleUI.ShowFileResult(outputPath);
    }
}