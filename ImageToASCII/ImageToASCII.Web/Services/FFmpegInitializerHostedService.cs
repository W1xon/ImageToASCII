using ImageToASCII.Services;

namespace ImageToASCII.Web;

public class FFmpegInitializerHostedService : IHostedService
{

    private readonly FFmpegBootstrapper _bootstrapper;

    public FFmpegInitializerHostedService(FFmpegBootstrapper bootstrapper)
    {
        _bootstrapper = bootstrapper;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_bootstrapper.IsExist) return;
        string ffmpegDir = Path.Combine(AppContext.BaseDirectory, "ffmpeg");
        await _bootstrapper.EnsureFFmpegAsync(ffmpegDir);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}