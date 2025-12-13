namespace ImageToASCII;

class Program
{
    [STAThread]
    static void Main()
    {
        Task.Run(async () => await RunAsync()).GetAwaiter().GetResult();
    }
    static async Task RunAsync()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "ImageToASCII Converter";
        
        var app = new Application.Application();
        await app.RunAsync();
    }
}