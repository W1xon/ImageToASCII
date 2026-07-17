using ImageToASCII.UI;

namespace ImageToASCII.Services;

public class LinuxDialogHelper
{
    public static string? GetPath(string filter, string title)
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
}