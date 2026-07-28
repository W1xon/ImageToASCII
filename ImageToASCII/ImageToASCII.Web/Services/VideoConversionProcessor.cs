using ImageToASCII.Core;
using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;
using ImageToASCII.Core.Processors;
using ImageToASCII.Services;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class VideoConversionProcessor : IConversionProcessor
{
    private readonly FFmpegBootstrapper _bootstrapper;
    public VideoConversionProcessor(FFmpegBootstrapper bootstrapper)
    {
        _bootstrapper = bootstrapper;
    }
    public async Task Process(ConversionJob job, IReporter reporter)
    {
        var converter = new BitmapToAsciiConverter(job.Settings.AsciiPalette.Characters.ToArray());
        using var exporter = new AsciiExporter(converter, reporter) { AsciiWidth = job.Settings.Width };
        
        var classifier = ColorClassifierFactory
            .Create(job.Settings.PaletteType);
        var videoConverter = new VideoToAsciiConverter(exporter, _bootstrapper, reporter);

        await videoConverter.InitializeAsync();

        reporter.ShowInfo("Начинаем конвертацию видео...");

        await videoConverter.Convert(
            job.Settings.InputFilePath,
            job.OutputPath,
            classifier);
    }
}