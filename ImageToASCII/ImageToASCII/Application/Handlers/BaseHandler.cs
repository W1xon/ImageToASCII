using ImageToASCII.Core.Models;
using ImageToASCII.UI;

namespace ImageToASCII.Application;

public abstract class BaseHandler
{
    protected ConversionSettings Settings { get; private set; }

    public async Task ProcessAsync()
    {
        Initialize();

        if (!TrySelectInputFile() || !CollectSettings())
        {
            ConsoleUI.WaitForKey();
            return;
        }

        try
        {
            await ExecuteConversionAsync();
        }
        catch (Exception exception)
        {
            ConsoleUI.ShowException(exception);
        }

        ConsoleUI.WaitForKey();
    }

    protected abstract string GetFileFilter();
    protected abstract string GetFileDialogTitle();
    protected abstract bool CollectSettings();
    protected abstract Task ExecuteConversionAsync();

    private void Initialize()
    {
        Console.Clear();
        ConsoleUI.DrawBanner();
        Settings = new ConversionSettings();
    }

    private bool TrySelectInputFile()
    {
        var inputFilePath = ConsoleUI.AskForFile(
            GetFileFilter(),
            GetFileDialogTitle());

        if (string.IsNullOrWhiteSpace(inputFilePath))
            return false;

        Settings.InputFilePath = inputFilePath;
        return true;
    }
}