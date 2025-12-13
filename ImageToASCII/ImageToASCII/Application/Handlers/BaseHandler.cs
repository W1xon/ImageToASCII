using ImageToASCII.Core.Models;
using ImageToASCII.UI;

namespace ImageToASCII.Application;

public abstract class BaseHandler
{
    protected ConversionSettings Settings { get; private set; }
    
    public async Task ProcessAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBanner();
        
        Settings = new ConversionSettings();
        if (!SelectFile())
        {
            ConsoleUI.WaitForKey();
            return;
        }
        
        if (!CollectSettings())
        {
            ConsoleUI.WaitForKey();
            return;
        }
        
        try
        {
            await ExecuteConversionAsync();
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowException(ex);
        }
        
        ConsoleUI.WaitForKey();
    }
    
    protected abstract string GetFileFilter();
    protected abstract string GetFileDialogTitle();
    protected abstract bool CollectSettings();
    protected abstract Task ExecuteConversionAsync();
    
    private bool SelectFile()
    {
        var selectedFile = ConsoleUI.AskForFile(GetFileFilter(), GetFileDialogTitle());
        
        if (selectedFile == null)
            return false;
        
        Settings.InputFilePath = selectedFile;
        return true;
    }
}