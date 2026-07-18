using ImageToASCII.Core.Models;

namespace ImageToASCII.Web.Models;

public enum JobType
{
    ImageToAscii,
    VideoToAscii
}

public class ConversionJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobType Type { get; init; }
    public ConversionSettings Settings { get; init; }
    public string OutputPath { get; init; }
    public TaskCompletionSource CompletionSource { get; } = new();
}