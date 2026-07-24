using ImageToASCII.ColorSystem;
using Microsoft.AspNetCore.Mvc;

namespace ImageToASCII.Web.DTOs;

public record ConvertMediaRequest
(
    [FromForm] IFormFile File, 
    [FromForm] PaletteType PaletteType, 
    [FromForm] int PaletteIndex, 
    [FromForm] int Width
);