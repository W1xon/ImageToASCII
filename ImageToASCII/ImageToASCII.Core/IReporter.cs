namespace ImageToASCII.Core;

public interface IReporter
{
    public void ShowInfo(string message) ;
    public void ShowProgress(int percent);
    public void ShowSuccess(string message);
    public void ShowWarning(string message);
    public void ShowError(string message);
    public void ShowHeader(string message);
}