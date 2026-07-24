using ImageToASCII.Web.DTOs;
using ImageToASCII.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageToASCII.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class MediaController : ControllerBase
{
    private readonly FileSignatureValidator _validator;
    private readonly ConvertMediaService _convertMediaService;
    public MediaController(ConvertMediaService convertMediaService, FileSignatureValidator validator)
    {
        _validator = validator;
        _convertMediaService = convertMediaService;
    }
    
    [HttpGet("convert")]
    public IActionResult SavePage()
    {
        return PhysicalFile(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload.html"),
            "text/html");
    }

    [HttpGet("status/{jobId:guid}")]
    public IActionResult GetStatus(Guid jobId)
    {
        var status = _convertMediaService.GetStatus(jobId);
        if (status == JobStatus.NotFound) return NotFound();
        return Ok(new { status = status.ToString() });
    }

    [HttpGet("result/{jobId:guid}")]
    [HttpHead("result/{jobId:guid}")]
    public IActionResult GetResult(Guid jobId)
    {
        var result = _convertMediaService.GetResult(jobId);
        if (result is null || !System.IO.File.Exists(result.OutputPath)) 
            return NotFound("Файл не найден либо еще не готов");

        return PhysicalFile(result.OutputPath, result.ContentType, enableRangeProcessing: true);
    }
    
    [RequestSizeLimit(33554432)]
    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromForm] ConvertMediaRequest media)
    {
        var (isValid, jobKind) = await _validator.IsValidFile(media.File);
        if (!isValid) return BadRequest("Неверный формат файла");
    
        Guid jobId = await _convertMediaService.EnqueueJobAsync(media, jobKind);
    
        return Ok(new { jobId });
    }
}