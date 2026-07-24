namespace ImageToASCII.Web;

public class MediaCleanupWorker : BackgroundService
{
    private readonly ConversionQueue _queue;

    public MediaCleanupWorker(ConversionQueue queue)
    {
        _queue = queue;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(20));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                int count = _queue.DeleteUnusedJob();
                Console.WriteLine($"Кол-во удаленных Job: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении фоновой очистки неиспользуемых задач {ex}");
            }
        }
    }
}