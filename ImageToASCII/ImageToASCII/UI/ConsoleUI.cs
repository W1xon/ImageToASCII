using ImageToASCII.Core.Models;
using ImageToASCII.Services;

namespace ImageToASCII.UI;

public static class ConsoleUI
{
    public static void DrawBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
  ╔═══════════════════════════════════════════════════════╗
  ║                                                       ║
  ║        ██╗███╗   ███╗ █████╗  ██████╗ ███████╗        ║
  ║        ██║████╗ ████║██╔══██╗██╔════╝ ██╔════╝        ║
  ║        ██║██╔████╔██║███████║██║  ███╗█████╗          ║
  ║        ██║██║╚██╔╝██║██╔══██║██║   ██║██╔══╝          ║
  ║        ██║██║ ╚═╝ ██║██║  ██║╚██████╔╝███████╗        ║
  ║        ╚═╝╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝        ║
  ║                                                       ║
  ║         [>] ASCII Converter v2.0.0 [<]                ║
  ║                                                       ║
  ╚═══════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    public static int ShowMainMenu()
    {
        Console.Clear();
        DrawBanner();
        Console.WriteLine("  [1] Конвертация изображения → ASCII PNG");
        Console.WriteLine("  [2] Конвертация изображения → ASCII TXT");
        Console.WriteLine("  [3] Конвертация видео → ASCII MP4");
        Console.WriteLine("  [0] Выход");
        Console.WriteLine();
    
        return ReadChoice(0, 3, "Выберите пункт");
    }

    public static int AskPalette()
    {
        Console.WriteLine();
        WriteHeader("--- Выбор цветовой палитры");
        Console.WriteLine();
        
        var options = new[]
        {
            ("Basic", "Простые цвета"),
            ("Palette16", "16-цветная палитра"),
            ("Palette7", "7-цветная палитра"),
            ("Natural", "Естественные цвета")
        };
        
        for (int i = 0; i < options.Length; i++)
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{i + 1}]");
            Console.ResetColor();
            Console.Write($" {options[i].Item1,-12} ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"({options[i].Item2})");
            Console.ResetColor();
        }
        
        return ReadChoice(1, 4);
    }

    public static char[] AskAsciiPalette()
    {
        Console.WriteLine();
        WriteHeader("--- Выбор ASCII градиента");
        Console.WriteLine();
        
        var palettes = ASCIIPalette.GetAllWithDescriptions();
        int halfCount = (palettes.Count + 1) / 2;
        
        for (int i = 0; i < halfCount; i++)
        {
            var left = palettes[i];
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{i + 1,2}]");
            Console.ResetColor();
            Console.Write($" {left.Name,-11} ");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{string.Join("", left.Palette.Take(8))}]");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" {left.Description,-13}");
            Console.ResetColor();
            
            int rightIndex = i + halfCount;
            if (rightIndex < palettes.Count)
            {
                var right = palettes[rightIndex];
                Console.Write("    ");
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{rightIndex + 1,2}]");
                Console.ResetColor();
                Console.Write($" {right.Name,-11} ");
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{string.Join("", right.Palette.Take(8))}]");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($" {right.Description}");
                Console.ResetColor();
            }
            
            Console.WriteLine();
        }
        
        int choice = ReadChoice(1, palettes.Count);
        return palettes[choice - 1].Palette;
    }

    public static int AskWidth()
    {
        Console.WriteLine();
        WriteHeader("--- Ширина ASCII изображения");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Рекомендуется: 100-200 для деталей, 50-80 для компактности, max: 1000");
        Console.ResetColor();
        Console.WriteLine();
        
        while (true)
        {
            Console.Write("  Ширина символов: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            string input = Console.ReadLine() ?? "";
            Console.ResetColor();
            
            if (int.TryParse(input, out int width) && width > 0 && width <= 1000)
                return width;
            
            WriteError("  Введите число от 1 до 1000");
        }
    }

    public static string? AskForFile(string filter, string title)
    {
        Console.WriteLine();
        WriteInfo($"Открытие диалога выбора файла...");
        
        var selectedFile = DialogHelper.ShowOpenFileDialog(filter, title);
        
        if (selectedFile != null)
        {
            WriteSuccess($"Выбран: {Path.GetFileName(selectedFile)}");
        }
        else
        {
            WriteError("Файл не выбран");
        }
        
        return selectedFile;
    }

    public static void ShowFileResult(string outputPath)
    {
        Console.WriteLine();
        WriteSuccess($"Сохранено: {Path.GetFileName(outputPath)}");
        WriteInfo($"Путь: {outputPath}");
        Console.WriteLine();
    }

    public static void ShowException(Exception ex)
    {
        WriteError($"Ошибка: {ex.Message}");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  Тип: {ex.GetType().Name}");
        if (ex.StackTrace != null)
        {
            var stackLines = ex.StackTrace.Split('\n');
            Console.WriteLine($"  Stack: {stackLines[0].Trim()}");
        }
        Console.ResetColor();
    }

    private static int ReadChoice(int min, int max, string? prompt = null)
    {
        while (true)
        {
            Console.WriteLine();
            Console.Write(prompt != null ? $"  {prompt}: " : "  Ваш выбор: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            string input = Console.ReadLine() ?? "";
            Console.ResetColor();
            
            if (int.TryParse(input, out int val) && val >= min && val <= max)
                return val;
            
            WriteError($"  Введите число от {min} до {max}");
        }
    }

    public static void WriteHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteInfo(string text)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  {text}");
        Console.ResetColor();
    }

    public static void WriteSuccess(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [OK] {text}");
        Console.ResetColor();
    }

    public static void WriteError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [!] {text}");
        Console.ResetColor();
    }

    public static void ShowProgress(string text)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("  [~] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WaitForKey()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Нажмите любую клавишу для продолжения...");
        Console.ResetColor();
        Console.ReadKey(true);
    }
    
    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  [!] {message}");
        Console.ResetColor();
    }
    public static void ShowInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  [i] {message}");
        Console.ResetColor();
    }
}