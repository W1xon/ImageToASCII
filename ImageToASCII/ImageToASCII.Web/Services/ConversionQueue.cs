using System.Collections.Concurrent;
using System.Threading.Channels;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class ConversionQueue
{
    public readonly Channel<ConversionJob> ConversionChannel = Channel.CreateUnbounded<ConversionJob>(new UnboundedChannelOptions
    {
        SingleReader = false  
    });

    private readonly ConcurrentDictionary<Guid, ConversionJob> _jobs = new();
    public async Task EnqueueWorkAsync(ConversionJob job)
    {
        _jobs[job.Id] = job;
         await ConversionChannel.Writer.WriteAsync(job);
    }

    public ConversionJob? GetJob(Guid id)
    {
        _jobs.TryGetValue(id, out var job);
        return job;
    }

    public void RemoveJob(Guid id)
    {
        _jobs.TryRemove(id, out var job);
        if (job is null)
            return;

        if (File.Exists(job.Settings.InputFilePath))
            File.Delete(job.Settings.InputFilePath);
        if(File.Exists(job.OutputPath))
            File.Delete(job.OutputPath);
    }

    public int DeleteUnusedJob()
    {
        var expiredIds = _jobs.Values
            .Where(j => j.Status == JobStatus.Failed ||  j.IsExpired)
            .Select(j => j.Id)
            .ToList();
        
        foreach (var id in expiredIds)
        {
            RemoveJob(id);
        }
        return expiredIds.Count;
    }
}