using ImageToASCII.Core.Models;

namespace ImageToASCII.Web.Models;

public enum JobType
{
    ImageToAscii,
    VideoToAscii
}

public enum JobStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    NotFound
}

public class ConversionJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public JobType Type { get; init; }
    public string ContentType { get; init; } = "";
    public ConversionSettings Settings { get; init; }
    public string OutputPath { get; init; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }

    public void MarkAsCompleted(TimeSpan retentionPeriod)
    {
        Status = JobStatus.Completed;
        ExpiresAt = DateTime.UtcNow.Add(retentionPeriod);
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}