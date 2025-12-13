using ImageToASCII.UI;
using Microsoft.Win32;

namespace ImageToASCII.Services;

public class DialogHelper
{
    public static string? ShowOpenFileDialog(string filter, string title)
    {
        string? result = null;
        
        var thread = new Thread(() =>
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = filter,
                    Title = title
                };
                
                if (dialog.ShowDialog() == true)
                {
                    result = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteError($"Ошибка диалога: {ex.Message}");
            }
        });
        
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        
        return result;
    }
}