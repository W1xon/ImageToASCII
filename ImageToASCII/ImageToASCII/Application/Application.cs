using ImageToASCII.UI;

namespace ImageToASCII.Application;

public class Application
{
    private readonly ImageToAsciiHandler _imageHandler;
    private readonly ImageToTextHandler _textHandler;
    private readonly VideoToAsciiHandler _videoHandler;
    
    public Application()
    {
        _imageHandler = new ImageToAsciiHandler();
        _textHandler = new ImageToTextHandler();
        _videoHandler = new VideoToAsciiHandler();
    }
    
    public async Task RunAsync()
    {
        while (true)
        {
            int choice = ConsoleUI.ShowMainMenu();
            
            if (choice == 0) break;
            
            try
            {
                switch (choice)
                {
                    case 1:
                        await _imageHandler.ProcessAsync();
                        break;
                    case 2:
                        await _textHandler.ProcessAsync();
                        break;
                    case 3:
                        await _videoHandler.ProcessAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowException(ex);
                ConsoleUI.WaitForKey();
            }
        }
    }
}
