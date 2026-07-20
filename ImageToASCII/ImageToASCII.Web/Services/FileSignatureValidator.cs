using ImageToASCII.Web.Models;

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
        
        //начиная с 7 номера видеоформаты
        { ".mp4",
            [
                [0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70], // ftyp offset=0, box size 0x18
                [0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70], // box size 0x1C
                [0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70], // box size 0x20
            ]
        },
        { ".mov",
            [
                [0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70], // ftyp qt
                [0x66, 0x72, 0x65, 0x65], // free atom variant, offset=4
                [0x6D, 0x64, 0x61, 0x74], // mdat variant, offset=4
            ]
        },
        { ".avi", [ [0x52, 0x49, 0x46, 0x46] ] }, // RIFF, нужна доп проверка "AVI " на offset 8
        { ".webm", [ [0x1A, 0x45, 0xDF, 0xA3] ] }, // EBML header
        { ".mkv", [ [0x1A, 0x45, 0xDF, 0xA3] ] }, // тот же EBML, MKV и WEBM неразличимы по magic bytes
        { ".flv", [ [0x46, 0x4C, 0x56, 0x01] ] }, // "FLV" + version 1
        { ".wmv", [ [0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11] ] }, // ASF header GUID
    };
    private static readonly Dictionary<string, JobType> ExtensionToJobType = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".bmp", JobType.ImageToAscii },
        { ".png", JobType.ImageToAscii },
        { ".jpg", JobType.ImageToAscii },
        { ".jpeg", JobType.ImageToAscii },
        { ".gif", JobType.ImageToAscii },
        { ".webp", JobType.ImageToAscii },

        { ".mp4", JobType.VideoToAscii },
        { ".mov", JobType.VideoToAscii },
        { ".avi", JobType.VideoToAscii },
        { ".webm", JobType.VideoToAscii },
        { ".mkv", JobType.VideoToAscii },
        { ".flv", JobType.VideoToAscii },
        { ".wmv", JobType.VideoToAscii },
    };
    public async Task<(bool IsValid, JobType JobKind)> IsValidFile(IFormFile file)
    {
        if (file is null || file.Length <= 0)
            return (false, JobType.ImageToAscii);
        
        string fileExtension = Path.GetExtension(file.FileName);
        
        if (!AllowedExtensionsAndSignatures.TryGetValue(fileExtension, out var signatures))
            return (false, JobType.ImageToAscii);

        int maxLength = signatures.Max(s => s.Length);
        using var stream = file.OpenReadStream();
        
        byte[] headerBuffer = new byte[maxLength];
        int byteReads = await stream.ReadAsync(headerBuffer.AsMemory(0, maxLength));
        bool isValid =  signatures.Any(s =>
            byteReads >= s.Length &&
            headerBuffer.AsSpan(0, s.Length).SequenceEqual(s));
        return (isValid, ExtensionToJobType[fileExtension]);
    }
}