using ImageToASCII.UI;

#if WINDOWS
using System.Windows.Forms;
#endif

namespace ImageToASCII.Services;

public class DialogHelper
{
    public static string? ShowOpenFileDialog(string filter, string title)
    {
#if WINDOWS
        return ShowWindowsDialog(filter, title);
#else
        return ShowLinuxPrompt(title);
#endif
    }

#if WINDOWS
    private static string? ShowWindowsDialog(string filter, string title)
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
#else
    private static string? ShowLinuxPrompt(string title)
    {
        ConsoleUI.ShowInfo($"{title} (перетащите файл или введите путь):");
        string? path = Console.ReadLine()?.Trim().Trim('\'').Trim('"');

        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (!File.Exists(path))
        {
            ConsoleUI.WriteError($"Файл не найден: {path}");
            return null;
        }

        return path;
    }
#endif
}