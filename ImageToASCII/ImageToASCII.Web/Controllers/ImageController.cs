using ImageToASCII.Core.Models;
using ImageToASCII.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageToASCII.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private static readonly Dictionary<string, byte[]> AllowedExtensionsAndSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg",  new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png",  new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".gif",  new byte[] { 0x47, 0x49, 0x46 } },
    };

    private readonly ConversionQueue _queue;
    
    public ImageController(ConversionQueue queue)
    {
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

    [RequestSizeLimit(33554432)]
    [HttpPost("save")]
    public async Task<IActionResult> Save([FromForm] IFormFile file)
    {
        
        if (file is null || file.Length <= 0)
            return BadRequest("Файл не выбран.");
        string uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        string fileExtension = Path.GetExtension(file.FileName);
        
        if (!AllowedExtensionsAndSignatures.ContainsKey(fileExtension))
        {
            return BadRequest("Недопустимый тип файла. Разрешены только JPG, PNG, GIF.");
        }
        
        bool isValidMedia = false;
        using (var stream = file.OpenReadStream())
        {
            
            byte[] requiredSignature = AllowedExtensionsAndSignatures[fileExtension];
            byte[] headerBuffer = new byte[requiredSignature.Length];
            int byteReads = await stream.ReadAsync(headerBuffer, 0, requiredSignature.Length);
            
            if (byteReads >= requiredSignature.Length && headerBuffer.Take(requiredSignature.Length).SequenceEqual(requiredSignature))
            {
                isValidMedia = true;
            }
        }

        if (!isValidMedia)
            return BadRequest("Содержимое не соответсвует заявленому");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);
        
        string safeFileName = $"{Guid.NewGuid()}{fileExtension}";
        string filePath = Path.Combine(uploadsDir, safeFileName);
        Console.WriteLine(filePath);
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        await ConvertImg(filePath);
        
        return Ok(new { message = "Файл успешно сохранен", fileName = file.FileName, fullPath = filePath });
    }

    private async Task ConvertImg(string filePath)
    {
        var settings = new ConversionSettings
        {
            InputFilePath = filePath,
            Width = 150,
        };

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