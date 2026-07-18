using ImageToASCII.Core;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class ConversionImageWorker : BackgroundService
{
    private ConversionQueue _queue;
    private IReporter _reporter;
    public ConversionImageWorker(ConversionQueue queue, IReporter reporter)
    {
        _queue = queue;
        _reporter = reporter;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var job in _queue.ConversionChannel.Reader.ReadAllAsync())
            {
                ProcessJobAsync(job, stoppingToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Обработчик сломался: {e.Message}");
        }
    }

    private async Task ProcessJobAsync(ConversionJob job, CancellationToken stoppingToken)
    {
        try
        {
            var processor = ConversionProcessorFactory.Create(job.Type);
            processor.Process(job, _reporter);
            job.CompletionSource.SetResult();
        }
        catch (Exception ex)
        {
            job.CompletionSource.SetException(ex);
        }
    }
}