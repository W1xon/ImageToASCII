using System.Drawing;
using System.Drawing.Imaging;
using ImageToASCII.ColorClassifiers;
using ImageToASCII.ImageProcessors;
using Microsoft.Win32;

namespace ImageToASCII;

class Program
{
    private static BitmapToASCIIConverter _asciiConverter;
    private static AsciiImageRenderer _asciiImageRenderer;
    private static AsciiExporter _asciiExporter;
    private static string _baseName;

    [STAThread]
    static void Main()
    {
        Init();
        Console.Title = "ImageToASCII";

        while (true)
        {
            Console.Clear();
            WriteHeader("Image → ASCII");

            int palette = AskPalette();
            int width = AskWidth();

            _asciiExporter.AsciiWidth = width;

            var bitmap = OpenImage();
            if (bitmap == null)
            {
                WriteError("Файл не открыт.");
                Thread.Sleep(1000);
                continue;
            }

            Console.Clear();
            WriteInfo("Генерация ASCII...");

            var asciiBitmap = GenerateAscii(bitmap, palette);
            string outputName = BuildOutputName(palette, width);

            asciiBitmap.Save(outputName, ImageFormat.Png);

            WriteSuccess($"Готово: {outputName}");
            Console.WriteLine("Нажмите Enter чтобы продолжить...");
            Console.ReadLine();
        }
    }

    private static void Init()
    {
        _asciiConverter = new BitmapToASCIIConverter();
        _asciiExporter = new AsciiExporter(_asciiConverter);
        _asciiImageRenderer = new AsciiImageRenderer(_asciiConverter);
    }

    private static int AskPalette()
    {
        while (true)
        {
            Console.WriteLine("Выберите палитру:");
            Console.WriteLine(" 1) Basic");
            Console.WriteLine(" 2) Palette16");
            Console.WriteLine(" 3) Palette7");
            Console.WriteLine(" 4) Natural");
            Console.Write("\nВведите номер: ");

            if (int.TryParse(Console.ReadLine(), out int val) &&
                val is >= 1 and <= 4)
                return val;
        }
    }

    private static int AskWidth()
    {
        while (true)
        {
            Console.Write("\nВведите ширину ASCII изображения: ");
            if (int.TryParse(Console.ReadLine(), out int width) && width > 0)
                return width;
        }
    }

    private static Bitmap? OpenImage()
    {
        Bitmap? bitmap = null;

        var dialog = new OpenFileDialog()
        {
            Filter = "Images | *.bmp; *.png; *.jpg; *.jpeg"
        };

        if (dialog.ShowDialog() == true)
        {
            bitmap = new Bitmap(dialog.FileName);
            _baseName = Path.GetFileName(dialog.FileName);
        }

        return bitmap;
    }

    private static Bitmap GenerateAscii(Bitmap source, int palette)
    {
        return _asciiExporter.PrintAndSave(
            source,
            ColorClassifierRegistry.GetClassifier(palette)
        );
    }

    private static string BuildOutputName(int palette, int width)
    {
        string paletteName = palette switch
        {
            1 => "basic",
            2 => "palette16",
            3 => "palette7",
            4 => "natural",
            _ => "unknown"
        };

        string clean = Path.GetFileNameWithoutExtension(_baseName);

        string outputDir = Path.Combine(AppContext.BaseDirectory, "Output");
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        return Path.Combine(outputDir, $"{clean}_ascii_{paletteName}_{width}px.png");
    }


    private static void WriteHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("────────────────────────────────────────────");
        Console.WriteLine($" {text}");
        Console.WriteLine("────────────────────────────────────────────");
        Console.ResetColor();
    }

    private static void WriteInfo(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static void WriteSuccess(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static void WriteError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
