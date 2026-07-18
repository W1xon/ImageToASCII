using ImageToASCII.Core;

namespace ImageToASCII.UI;

public class ConsoleReporter : IReporter
{
    public void ShowInfo(string message) => ConsoleUI.ShowInfo(message);
    public void ShowProgress(int percent) => Console.Write($"\r  Прогресс: {percent}%   ");
    public void ShowSuccess(string message) => ConsoleUI.WriteSuccess(message);
    public void ShowWarning(string message) => ConsoleUI.WriteWarning(message);
    public void ShowError(string message) => ConsoleUI.WriteError(message);
    public void ShowHeader(string message) => ConsoleUI.WriteHeader(message);
}