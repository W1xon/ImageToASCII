using ImageToASCII.Core;

namespace ImageToASCII.Web;

public class WebReporter : IReporter
{
    public void ShowInfo(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowProgress(int percent)
    {
        Console.WriteLine(percent);
    }

    public void ShowSuccess(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowWarning(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowError(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowHeader(string message)
    {
        Console.WriteLine(message);
    }
}