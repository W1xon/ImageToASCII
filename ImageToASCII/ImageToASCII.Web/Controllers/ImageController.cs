using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Models;
using ImageToASCII.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageToASCII.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly ConversionQueue _queue;
    private readonly FileSignatureValidator _validator;
    
    public ImageController(ConversionQueue queue, FileSignatureValidator validator)
    {
        _validator = validator;
        _queue = queue;
    }
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "hello" });
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        return Ok(new { id });
    }
    
    [HttpGet("save")]
    public IActionResult SavePage()
    {
        return PhysicalFile(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload.html"),
            "text/html");
    }
    
    [RequestSizeLimit(33554432)]
    [HttpPost("save")]
    public async Task<IActionResult> Save(
        [FromForm] IFormFile file, 
        [FromForm] PaletteType paletteType, 
        [FromForm] int paletteIndex, 
        [FromForm] int width)
    {
        bool isValid = await _validator.IsValidFile(file);
        
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
        await ConvertImg(filePath, settings);
        
        return Ok(new { message = "Файл успешно сохранен", fileName = file.FileName, fullPath = filePath });
    }
    private async Task ConvertImg(string filePath, ConversionSettings settings)
    {

        var job = new ConversionJob
        {
            Type = JobType.ImageToAscii,
            Settings = settings,
            OutputPath = $"{filePath}"
        };

        await _queue.EnqueueAsync(job);
        await job.CompletionSource.Task;
    }
}