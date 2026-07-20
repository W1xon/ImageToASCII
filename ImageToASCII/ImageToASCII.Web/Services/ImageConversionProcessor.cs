using ImageToASCII.ColorSystem;
using ImageToASCII.Core;
using ImageToASCII.Core.Converters;
using ImageToASCII.Core.Processors;
using ImageToASCII.Web.Models;
using SkiaSharp;

namespace ImageToASCII.Web;

public class ImageConversionProcessor : IConversionProcessor
{
    public async Task Process(ConversionJob job, IReporter reporter)
    {
        var converter = new BitmapToAsciiConverter(job.Settings.AsciiPalette.Characters.ToArray());
        var exporter = new AsciiExporter(converter, reporter) { AsciiWidth = job.Settings.Width };

        using var sourceBitmap = SKBitmap.Decode(job.Settings.InputFilePath);
        var classifier = ColorClassifierFactory.Create(job.Settings.PaletteType);

        using var asciiBitmap = exporter.GetProcessing(sourceBitmap, classifier);
        using var image = SKImage.FromBitmap(asciiBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(job.OutputPath);
        data.SaveTo(stream);
    }
}