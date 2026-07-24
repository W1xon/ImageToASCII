using ImageToASCII.Services;
using ImageToASCII.UI;

namespace ImageToASCII.Application;

public sealed class Application
{
    private readonly Dictionary<int, BaseHandler> _handlers;
    private readonly FFmpegBootstrapper _bootstrapper;
    public Application()
    {
        ConsoleReporter consoleReporter = new ConsoleReporter();
        _bootstrapper = new FFmpegBootstrapper(consoleReporter);
        
        _handlers = new Dictionary<int, BaseHandler>
        {
            [1] = new ImageToAsciiHandler(consoleReporter),
            [2] = new ImageToTextHandler(consoleReporter),
            [3] = new VideoToAsciiHandler(consoleReporter, _bootstrapper)
        };
    }

    public async Task RunAsync()
    {
        await InitFFmpeg();
        while (true)
        {
            var choice = ConsoleUI.ShowMainMenu();

            if (choice == 0)
                break;

            if (!_handlers.TryGetValue(choice, out var handler))
                continue;

            try
            {
                await handler.ProcessAsync();
            }
            catch (Exception exception)
            {
                ConsoleUI.ShowException(exception);
                ConsoleUI.WaitForKey();
            }
        }
    }

    private async Task InitFFmpeg()
    {
        if (_bootstrapper.IsExist) return;
        string ffmpegDir = Path.Combine(AppContext.BaseDirectory, "ffmpeg");
        await _bootstrapper.EnsureFFmpegAsync(ffmpegDir);
    }
}