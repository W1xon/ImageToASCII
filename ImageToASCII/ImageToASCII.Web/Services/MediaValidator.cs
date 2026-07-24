using ImageToASCII.Web.DTOs;
using ImageToASCII.Web.Models;
using SixLabors.ImageSharp;

namespace ImageToASCII.Web;

public class MediaValidator
{
    private record SignatureRule(int Offset, byte[] Bytes);

    private static readonly Dictionary<string, List<SignatureRule>> AllowedSignatures =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { ".bmp",  [ new(0, [0x42, 0x4D]) ] },
        { ".png",  [ new(0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]) ] },
        { ".jpg",  [ new(0, [0xFF, 0xD8]) ] },
        { ".jpeg", [ new(0, [0xFF, 0xD8]) ] },
        { ".gif",  [ new(0, [0x47, 0x49, 0x46, 0x38, 0x37, 0x61]), new(0, [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]) ] },
        { ".webp", [ new(0, [0x52, 0x49, 0x46, 0x46]), new(8, [0x57, 0x45, 0x42, 0x50]) ] },

        { ".mp4",  [ new(4, [0x66, 0x74, 0x79, 0x70]) ] },
        { ".mov",  [ new(4, [0x66, 0x74, 0x79, 0x70]), new(4, [0x66, 0x72, 0x65, 0x65]), new(4, [0x6D, 0x64, 0x61, 0x74]) ] },
        { ".avi",  [ new(0, [0x52, 0x49, 0x46, 0x46]), new(8, [0x41, 0x56, 0x49, 0x20]) ] },
        { ".webm", [ new(0, [0x1A, 0x45, 0xDF, 0xA3]) ] },
        { ".mkv",  [ new(0, [0x1A, 0x45, 0xDF, 0xA3]) ] },
        { ".flv",  [ new(0, [0x46, 0x4C, 0x56, 0x01]) ] },
        { ".wmv",  [ new(0, [0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11]) ] },
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

    private const double MAX_VIDEO_DURATION_SECONDS = 20.0;
    private readonly FfprobeMediaInspector _mediaInspector;

    public MediaValidator(FfprobeMediaInspector mediaInspector)
    {
        _mediaInspector = mediaInspector;
    }

    public async Task<(bool IsValid, JobType JobKind, string? ErrorReason)> ValidateAsync(ConvertMediaRequest media)
    {
        var file = media.File;
        if (file is null || file.Length == 0)
            return (false, JobType.ImageToAscii, "Загружен пустой файл");

        string ext = Path.GetExtension(file.FileName);
        if (!ExtensionToJobType.TryGetValue(ext, out var jobKind))
            return (false, JobType.ImageToAscii, "Неподдерживаемое расширение файла");

        if (!await HasValidMagicBytesAsync(file, ext))
            return (false, jobKind, "Заголовок файла поврежден или некорректен");

        if (jobKind == JobType.ImageToAscii)
        {
            var isImageValid = await ValidateImageContentAsync(file);
            if (!isImageValid)
                return (false, jobKind, "Файл не является корректным изображением");

            return (true, jobKind, null);
        }

        if (jobKind == JobType.VideoToAscii)
        {
            if (media.Width > 100)
                return (false, jobKind, "Для видео не поддерживается размер больше 100");

            var duration = await _mediaInspector.GetVideoDurationSecondsAsync(file);
            if (duration is null)
                return (false, jobKind, "Не удалось прочитать медиапоток видеофайла");

            if (duration > MAX_VIDEO_DURATION_SECONDS)
                return (false, jobKind, $"Длительность видео ({Math.Round(duration.Value)} сек) превышает лимит в {MAX_VIDEO_DURATION_SECONDS} сек");
        }

        return (true, jobKind, null);
    }

    private async Task<bool> HasValidMagicBytesAsync(IFormFile file, string ext)
    {
        if (!AllowedSignatures.TryGetValue(ext, out var rules))
            return false;

        int maxBytesNeeded = rules.Max(r => r.Offset + r.Bytes.Length);
        using var stream = file.OpenReadStream();
        byte[] buffer = new byte[maxBytesNeeded];
        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, maxBytesNeeded));

        ReadOnlySpan<byte> span = buffer.AsSpan(0, bytesRead);

        foreach (var rule in rules)
        {
            if (span.Length >= rule.Offset + rule.Bytes.Length &&
                span.Slice(rule.Offset, rule.Bytes.Length).SequenceEqual(rule.Bytes))
            {
                return true;
            }
        }

        return false;
    }


    private static async Task<bool> ValidateImageContentAsync(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            IImageInfo info = await Image.IdentifyAsync(stream);
            if (info.Width > 7096 || info.Height > 7096)
            {
                return false; 
            }
            return info != null && info.Width > 0 && info.Height > 0;
        }
        catch
        {
            return false;
        }
    }
}