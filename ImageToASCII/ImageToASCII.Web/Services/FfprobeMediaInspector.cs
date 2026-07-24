using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using ImageToASCII.Services;

namespace ImageToASCII.Web;

public class FfprobeMediaInspector
{
    private readonly FFmpegBootstrapper _bootstrapper;

    public FfprobeMediaInspector(FFmpegBootstrapper bootstrapper)
    {
        _bootstrapper = bootstrapper;
    }

    public async Task<double?> GetVideoDurationSecondsAsync(IFormFile file, CancellationToken ct = default)
    {
        string ffprobeExe = Path.Combine(_bootstrapper.GetFFmpegPath(), RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");

        var psi = new ProcessStartInfo
        {
            FileName = ffprobeExe,
            Arguments = "-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 pipe:0",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var readOutputTask = process.StandardOutput.ReadToEndAsync(ct);

        try
        {
            await using var stdinStream = process.StandardInput.BaseStream;
            await using var fileStream = file.OpenReadStream();
            await fileStream.CopyToAsync(stdinStream, ct);
        }
        catch (IOException) {  }

        string output = await readOutputTask;
        await process.WaitForExitAsync(ct);

        if (double.TryParse(output.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double duration))
        {
            return duration;
        }

        return null;
    }
}