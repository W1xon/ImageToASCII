using ImageToASCII.Core;

namespace ImageToASCII.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<ConversionQueue>();
        builder.Services.AddSingleton<IReporter, WebReporter>();
        builder.Services.AddHostedService<ConversionImageWorker>();
        builder.Services.AddControllers();

        var app = builder.Build();
        app.MapControllers();
        app.Run();
    }
}