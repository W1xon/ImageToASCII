namespace ImageToASCII;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "ImageToASCII Converter";
        
        var app = new Application.Application();
        await app.RunAsync();
    }
}