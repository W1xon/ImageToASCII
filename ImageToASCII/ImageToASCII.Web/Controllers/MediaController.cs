using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Models;
using ImageToASCII.Services;
using ImageToASCII.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageToASCII.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class MediaController : ControllerBase
{
    private readonly ConversionQueue _queue;
    private readonly FileSignatureValidator _validator;
    
    public MediaController(ConversionQueue queue, FileSignatureValidator validator)
    {
        _validator = validator;
        _queue = queue;
    }
    
    [HttpGet("convert")]
    public IActionResult SavePage()
    {
        return PhysicalFile(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload.html"),
            "text/html");
    }
    
    [RequestSizeLimit(33554432)]
    [HttpPost("convert")]
    public async Task<IActionResult> Save(
        [FromForm] IFormFile file, 
        [FromForm] PaletteType paletteType, 
        [FromForm] int paletteIndex, 
        [FromForm] int width)
    {
        var (isValid, jobKind) = await _validator.IsValidFile(file);
        
        if (!isValid) return BadRequest("Неверный формат файла") ;
        string uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);
        
        string safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsDir, safeFileName);
        Console.WriteLine(filePath);
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        paletteIndex = AsciiPaletteRegistry.All.Count < paletteIndex ? AsciiPaletteRegistry.All.Count : paletteIndex;
        var settings = new ConversionSettings
        {
            InputFilePath = filePath,
            Width = width,
            PaletteType = paletteType,
            AsciiPalette = AsciiPaletteRegistry.All[paletteIndex]
        };
        string outputFilePath = jobKind == JobType.ImageToAscii
            ? new OutputNameBuilder(settings)
                .BuildImage()
            : new OutputNameBuilder(settings)
                .BuildVideo();
        
        await ConvertImg(outputFilePath, settings, jobKind);
        string contentType = file.ContentType;
        try
        {
              var stream = new FileStream(
                outputFilePath,FileMode.Open,
                FileAccess.Read,
                FileShare.Read | FileShare.Delete,
                4096,
                FileOptions.DeleteOnClose);
            
            return File(stream, contentType);
        }
        finally
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
    private async Task ConvertImg(string filePath, ConversionSettings settings, JobType jobType)
    {

        var job = new ConversionJob
        {
            Type = jobType,
            Settings = settings,
            OutputPath = filePath
        };

        await _queue.EnqueueAsync(job);
        await job.CompletionSource.Task;
    }
}