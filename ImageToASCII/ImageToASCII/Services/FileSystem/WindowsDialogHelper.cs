#if WINDOWS
using ImageToASCII.UI;
using System.Windows.Forms;
namespace ImageToASCII.Services;

public class WindowsDialogHelper
{
    public static string? GetPath(string filter, string title)
    {
        
        string? result = null;

        var thread = new Thread(() =>
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = filter,
                    Title = title
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                    result = dialog.FileName;
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
#endif