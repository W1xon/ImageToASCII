namespace ImageToASCII.Web.Files;

public class FileSystemService
{
    public async Task<string> Save(IFormFile file)
    {
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
        return filePath;
    }
}