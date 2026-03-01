using ImageToASCII.UI;

namespace ImageToASCII.Application;

public sealed class Application
{
    private readonly Dictionary<int, BaseHandler> _handlers;

    public Application()
    {
        _handlers = new Dictionary<int, BaseHandler>
        {
            [1] = new ImageToAsciiHandler(),
            [2] = new ImageToTextHandler(),
            [3] = new VideoToAsciiHandler()
        };
    }

    public async Task RunAsync()
    {
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
}