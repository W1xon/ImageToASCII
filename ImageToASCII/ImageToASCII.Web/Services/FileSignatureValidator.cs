namespace ImageToASCII.Web;

public class FileSignatureValidator
{
    private static readonly Dictionary<string, List<byte[]>> AllowedExtensionsAndSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".bmp",  [ [0x42, 0x4D] ] }, // "BM"

        { ".png",  [ [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] ] },

        { ".jpg",
            [
                [0xFF, 0xD8, 0xFF, 0xDB], // raw/no APPn
                [0xFF, 0xD8, 0xFF, 0xE0], // JFIF
                [0xFF, 0xD8, 0xFF, 0xE1], // Exif
                [0xFF, 0xD8, 0xFF, 0xE2], // ICC/CIFF
                [0xFF, 0xD8, 0xFF, 0xE8], // SPIFF
                [0xFF, 0xD8, 0xFF, 0xEE], // Adobe
            ]
        },
        { ".jpeg",
            [
                [0xFF, 0xD8, 0xFF, 0xDB],
                [0xFF, 0xD8, 0xFF, 0xE0],
                [0xFF, 0xD8, 0xFF, 0xE1],
                [0xFF, 0xD8, 0xFF, 0xE2],
                [0xFF, 0xD8, 0xFF, 0xE8],
                [0xFF, 0xD8, 0xFF, 0xEE],
            ]
        },

        { ".gif",
            [
                [0x47, 0x49, 0x46, 0x38, 0x37, 0x61], // GIF87a
                [0x47, 0x49, 0x46, 0x38, 0x39, 0x61], // GIF89a
            ]
        },

        { ".webp", [ [0x52, 0x49, 0x46, 0x46] ] }, // "RIFF"
    };

    public async Task<bool> IsValidFile(IFormFile file)
    {
        if (file is null || file.Length <= 0)
            return false;
        
        string fileExtension = Path.GetExtension(file.FileName);
        
        if (!AllowedExtensionsAndSignatures.TryGetValue(fileExtension, out var signatures))
            return false;

        int maxLength = signatures.Max(s => s.Length);
        using var stream = file.OpenReadStream();
        
        byte[] headerBuffer = new byte[maxLength];
        int byteReads = await stream.ReadAsync(headerBuffer.AsMemory(0, maxLength));
        return signatures.Any(s =>
            byteReads >= s.Length &&
            headerBuffer.AsSpan(0, s.Length).SequenceEqual(s));
    }
}