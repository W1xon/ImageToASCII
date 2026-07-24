using ImageToASCII.ColorSystem;
using ImageToASCII.Core;
using ImageToASCII.Core.Processors;
using ImageToASCII.Services;
using ImageToASCII.UI;

namespace ImageToASCII.Application;

public sealed class VideoToAsciiHandler(IReporter reporter, FFmpegBootstrapper bootstrapper)
    : AsciiHandlerBase(reporter)
{
    private const string VideoFilter =
        "Видео|*.mp4;*.avi;*.mov;*.mkv|Все файлы|*.*";

    private const string DialogTitle = "Выберите видео";

    protected override string GetFileFilter() => VideoFilter;

    protected override string GetFileDialogTitle() => DialogTitle;

    protected override async Task ExecuteConversionAsync()
    {
        var classifier = ColorClassifierFactory
            .Create(Settings.PaletteType);

        var videoConverter = new VideoToAsciiConverter(AsciiExporter, bootstrapper, Reporter);

        await videoConverter.InitializeAsync();

        var outputPath = new OutputNameBuilder(Settings)
            .BuildVideo();

        ConsoleUI.ShowProgress("Начинаем конвертацию видео...");

        await videoConverter.Convert(
            Settings.InputFilePath,
            outputPath,
            classifier);

        ConsoleUI.ShowFileResult(outputPath);
    }
}