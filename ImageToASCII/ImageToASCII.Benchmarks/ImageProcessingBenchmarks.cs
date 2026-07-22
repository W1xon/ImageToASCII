using BenchmarkDotNet.Attributes;
using ImageToASCII.ColorSystem;
using ImageToASCII.Core;
using ImageToASCII.Core.Converters;
using ImageToASCII.Core.Processors;
using SkiaSharp;

[MemoryDiagnoser]
[RankColumn]
public class ImageProcessingBenchmarks
{
    private BitmapToAsciiConverter _asciiConverter;
    private IColorClassifier _classifier;
    private AsciiExporter _exporter;

    [ParamsSource(nameof(GetImagePaths))]
    public string ImagePath { get; set; } = string.Empty;

    private SKBitmap _sourceBitmap;

    public IEnumerable<string> GetImagePaths()
    {
        return Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Assets"), "*.*")
            .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg"));
    }

    [GlobalSetup]
    public void Setup()
    {
        char[] palette = ['.', ':', '-', '=', '+', '*', '#', '%', '@']; 
        _asciiConverter = new BitmapToAsciiConverter(palette);
        _classifier = ColorClassifierFactory.Create(PaletteType.Natural); 
        
        _exporter = new AsciiExporter(_asciiConverter, new NullReporter()) 
        { 
            AsciiWidth = 350 
        };
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _sourceBitmap = SKBitmap.Decode(ImagePath);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _sourceBitmap?.Dispose();
    }

    [Benchmark]
    public SKBitmap Benchmark_FullPipeline()
    {
        return _exporter.GetProcessing(_sourceBitmap, _classifier);
    }
}

public class NullReporter : IReporter
{
    public void ShowInfo(string message)
    {
    }

    public void ShowProgress(int percent)
    {
    }

    public void ShowSuccess(string message)
    {
    }

    public void ShowWarning(string message)
    {
    }

    public void ShowError(string message)
    {
    }

    public void ShowHeader(string message)
    {
    }
}