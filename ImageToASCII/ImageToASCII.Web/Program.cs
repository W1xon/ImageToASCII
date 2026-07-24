using ImageToASCII.Core;
using ImageToASCII.Services;
using ImageToASCII.Web.Files;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddSingleton<ConversionQueue>();
        builder.Services.AddSingleton<IReporter, WebReporter>();
        builder.Services.AddSingleton<MediaValidator>();
        builder.Services.AddSingleton<FileSystemService>();
        builder.Services.AddSingleton<ConvertMediaService>();
        builder.Services.AddSingleton<FFmpegBootstrapper>();
        builder.Services.AddSingleton<FfprobeMediaInspector>();
        
        builder.Services.AddKeyedSingleton<IConversionProcessor, ImageConversionProcessor>(JobType.ImageToAscii);
        builder.Services.AddKeyedSingleton<IConversionProcessor, VideoConversionProcessor>(JobType.VideoToAscii);
        
        builder.Services.AddHostedService<MediaConversionWorker>();
        builder.Services.AddHostedService<MediaCleanupWorker>();
        builder.Services.AddHostedService<FFmpegInitializerHostedService>();
        
        builder.Services.AddControllers();

        var app = builder.Build();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        
        app.MapControllers();
        app.Run();
    }
}