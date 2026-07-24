using System.Diagnostics;
using ImageToASCII.Core;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class MediaConversionWorker : BackgroundService
{
    private readonly ConversionQueue _queue;
    private readonly IReporter _reporter;
    private readonly IServiceProvider _serviceProvider;
    
    private int _maxParallelJob = Environment.ProcessorCount;
    public MediaConversionWorker(ConversionQueue queue, IReporter reporter, IServiceProvider serviceProvider)
    {
        _queue = queue;
        _reporter = reporter;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workers = new Task[_maxParallelJob];
        for (int i = 0; i < _maxParallelJob; i++)
        {
            workers[i] = StartWorkerAsync(stoppingToken);
        }
        await Task.WhenAll(workers);
    }
    private async Task StartWorkerAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var job in _queue.ConversionChannel.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessJob(job);
            }
        }
        catch(OperationCanceledException){}
        catch (Exception e)
        {
            Console.WriteLine($"Обработчик сломался: {e.Message}");
        }
    }
    private async Task ProcessJob(ConversionJob job)
    {
        var stopwatch = Stopwatch.StartNew();
    
        try
        {
            job.Status = JobStatus.Processing;
            var processor = _serviceProvider.GetRequiredKeyedService<IConversionProcessor>(job.Type);
        
            await processor.Process(job, _reporter);
        
            stopwatch.Stop();
            job.MarkAsCompleted(TimeSpan.FromSeconds(30));
            Console.WriteLine($"[Job {job.Id}] Успешно обработан за {stopwatch.ElapsedMilliseconds} мс (или {stopwatch.Elapsed.TotalSeconds:F2} сек)");
        }
        catch (Exception ex)
        {
            stopwatch.Stop(); 
            job.Status = JobStatus.Failed;
            Console.WriteLine($"[Job {job.Id}] Ошибка через {stopwatch.ElapsedMilliseconds} мс: {ex.Message}");
        }
    }
}