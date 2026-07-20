using System.Diagnostics;
using ImageToASCII.Core;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class ConversionImageWorker : BackgroundService
{
    private ConversionQueue _queue;
    private IReporter _reporter;
    private int _maxParallelJob = Environment.ProcessorCount;
    public ConversionImageWorker(ConversionQueue queue, IReporter reporter)
    {
        _queue = queue;
        _reporter = reporter;
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
            await foreach (var job in _queue.ConversionChannel.Reader.ReadAllAsync())
            {
                await Task.Run(() => ProcessJob(job), stoppingToken) ;
            }
        }
        catch(OperationCanceledException){}
        catch (Exception e)
        {
            Console.WriteLine($"Обработчик сломался: {e.Message}");
        }
    }
    private void ProcessJob(ConversionJob job)
    {
        var stopwatch = Stopwatch.StartNew();
    
        try
        {
            var processor = ConversionProcessorFactory.Create(job.Type);
        
            processor.Process(job, _reporter);
        
            stopwatch.Stop();
        
            Console.WriteLine($"[Job {job.Id}] Успешно обработан за {stopwatch.ElapsedMilliseconds} мс (или {stopwatch.Elapsed.TotalSeconds:F2} сек)");
        
            job.CompletionSource.SetResult();
        }
        catch (Exception ex)
        {
            stopwatch.Stop(); 
            Console.WriteLine($"[Job {job.Id}] Ошибка через {stopwatch.ElapsedMilliseconds} мс: {ex.Message}");
        
            job.CompletionSource.SetException(ex);
        }
    }
}