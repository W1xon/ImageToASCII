using ImageToASCII.Core.Models;
using ImageToASCII.Services;
using ImageToASCII.Web.DTOs;
using ImageToASCII.Web.Files;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class ConvertMediaService
{
    private FileSystemService _fileService;
    private ConversionQueue _queue;
    public ConvertMediaService(ConversionQueue queue, FileSystemService fileService)
    {
        _fileService = fileService;
        _queue = queue;
    }

    public async Task<Guid?> EnqueueJobAsync(ConvertMediaRequest media, JobType type)
    {
        string filePath = await _fileService.Save(media.File);
        int paletteIndex = AsciiPaletteRegistry.All.Count < media.PaletteIndex 
            ? AsciiPaletteRegistry.All.Count 
            : media.PaletteIndex;
        
        var settings = new ConversionSettings
        {
            InputFilePath = filePath,
            Width = media.Width,
            PaletteType = media.PaletteType,
            AsciiPalette = AsciiPaletteRegistry.All[paletteIndex]
        };
        string outputFilePath = type == JobType.ImageToAscii
            ? new OutputNameBuilder(settings)
                .BuildImage()
            : new OutputNameBuilder(settings)
                .BuildVideo();
        string contentType = type == JobType.ImageToAscii
            ? "image/png"
            : "video/mp4";
        var job = new ConversionJob
        {
            ContentType = contentType,
            Type = type,
            Settings = settings,
            OutputPath = outputFilePath,
            Status = JobStatus.Pending
        };
        
        if(_queue.TryEnqueueWork(job))
            return job.Id;
        return null;
    }

    public JobStatus GetStatus(Guid jobId)
    {
       var job = _queue.GetJob(jobId);
       if (job is null)
           return JobStatus.NotFound;
       return job.Status;
    }

    public ConversionJob? GetResult(Guid jobId)
    {
        var job = _queue.GetJob(jobId);
        if (job is null || job.Status != JobStatus.Completed)
            return null;
        return job;
    }
}