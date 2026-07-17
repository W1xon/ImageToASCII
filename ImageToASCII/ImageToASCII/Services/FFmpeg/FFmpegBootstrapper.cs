using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using ImageToASCII.UI;

namespace ImageToASCII.Services;

public static class FFmpegBootstrapper
{
    private const string FFMPEG_VERSION = "6.1";
    private const string TEMP_DIR_NAME = "temp_extract";

    private static bool _isReady;
    private static string? _ffmpegPath;

    private static readonly HttpClient _httpClient = new(
        new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private static string FfmpegExeName  => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe"  : "ffmpeg";
    private static string FfprobeExeName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe";

    public static async Task<bool> EnsureFFmpegAsync(string targetDir)
    {
        if (_isReady) return true;

        if (CheckLocalFiles(targetDir))
        {
            SetupPaths(targetDir);
            return _isReady = true;
        }

        ConfigureSecurity();
        Directory.CreateDirectory(targetDir);
        ConsoleUI.ShowProgress($"Загрузка FFmpeg {FFMPEG_VERSION}...");

        if (await TryDeployFFmpegAsync(targetDir))
        {
            SetupPaths(targetDir);
            return _isReady = true;
        }

        ShowFailureMessage(targetDir);
        return _isReady = false;
    }

    private static bool CheckLocalFiles(string targetDir) =>
        File.Exists(Path.Combine(targetDir, FfmpegExeName)) &&
        File.Exists(Path.Combine(targetDir, FfprobeExeName));

    private static void ConfigureSecurity()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
    }

    private static async Task<bool> TryDeployFFmpegAsync(string targetDir)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return await DeployWindowsAsync(targetDir);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return await DeployLinuxAsync(targetDir);

        ConsoleUI.WriteError("Платформа не поддерживается для автоматической загрузки FFmpeg.");
        return false;
    }

    private static async Task<bool> DeployWindowsAsync(string targetDir)
    {
        string zipPath = Path.Combine(targetDir, "ffmpeg.zip");
        string tempDir = Path.Combine(targetDir, TEMP_DIR_NAME);
        string url = $"https://github.com/GyanD/codexffmpeg/releases/download/{FFMPEG_VERSION}/ffmpeg-{FFMPEG_VERSION}-essentials_build.zip";

        try
        {
            await DownloadFileAsync(url, zipPath);
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            ZipFile.ExtractToDirectory(zipPath, tempDir);
            return LocateAndMoveBinaries(tempDir, targetDir, "ffmpeg.exe", "ffprobe.exe");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Ошибка (Windows): {ex.Message}");
            return false;
        }
        finally
        {
            Cleanup(zipPath, tempDir);
        }
    }

    private static async Task<bool> DeployLinuxAsync(string targetDir)
{
    string arch = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64   => "64",
        Architecture.Arm64 => "arm-64",
        _ => throw new PlatformNotSupportedException($"Архитектура {RuntimeInformation.ProcessArchitecture} не поддерживается.")
    };

    string gitHubVersion = "6.1";
    
    string ffmpegUrl  = $"https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v{gitHubVersion}/ffmpeg-{gitHubVersion}-linux-{arch}.zip";
    string ffprobeUrl = $"https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v{gitHubVersion}/ffprobe-{gitHubVersion}-linux-{arch}.zip";

    string ffmpegZip  = Path.Combine(targetDir, "ffmpeg-linux.zip");
    string ffprobeZip = Path.Combine(targetDir, "ffprobe-linux.zip");
    string tempDir    = Path.Combine(targetDir, TEMP_DIR_NAME);

    try
    {
        if (Directory.Exists(tempDir)) 
            Directory.Delete(tempDir, true);

        Directory.CreateDirectory(tempDir);
        
        ConsoleUI.ShowProgress("Загрузка ffmpeg...");
        await DownloadFileAsync(ffmpegUrl, ffmpegZip);

        ConsoleUI.ShowProgress("Загрузка ffprobe...");
        await DownloadFileAsync(ffprobeUrl, ffprobeZip);

        ConsoleUI.ShowProgress("Распаковка...");
        ZipFile.ExtractToDirectory(ffmpegZip, tempDir);
        ZipFile.ExtractToDirectory(ffprobeZip, tempDir);

        if (!LocateAndMoveBinaries(tempDir, targetDir, "ffmpeg", "ffprobe"))
        {
            ConsoleUI.WriteError("Не удалось перенести распакованные файлы.");
            return false;
        }

        MakeExecutable(Path.Combine(targetDir, "ffmpeg"));
        MakeExecutable(Path.Combine(targetDir, "ffprobe"));
        return true;
    }
    catch (Exception ex)
    {
        ConsoleUI.WriteError($"Ошибка (Linux): {ex.Message}");
        return false;
    }
    finally
    {
        Cleanup(ffmpegZip, tempDir);
        Cleanup(ffprobeZip, tempDir);
    }
}

    private static async Task DownloadFileAsync(string url, string destPath)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        await using var downloadStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[128 * 1024]; 
        long totalReadBytes = 0;
        int readBytes;
        int lastPercentage = -1; 

        while ((readBytes = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, readBytes);
            totalReadBytes += readBytes;

            if (totalBytes != -1)
            {
                int percentage = (int)((totalReadBytes * 100) / totalBytes);
            
                if (percentage != lastPercentage)
                {
                    lastPercentage = percentage;
                    ConsoleUI.ShowProgress($"Загрузка: {percentage}% ({totalReadBytes / 1024 / 1024}MB / {totalBytes / 1024 / 1024}MB)");
                }
            }
        }
    }

    private static bool LocateAndMoveBinaries(string sourceDir, string targetDir, string ffmpegName, string ffprobeName)
    {
        var ffmpeg  = Directory.EnumerateFiles(sourceDir, ffmpegName,  SearchOption.AllDirectories).FirstOrDefault();
        var ffprobe = Directory.EnumerateFiles(sourceDir, ffprobeName, SearchOption.AllDirectories).FirstOrDefault();

        if (ffmpeg == null || ffprobe == null) return false;

        File.Move(ffmpeg,  Path.Combine(targetDir, ffmpegName),  overwrite: true);
        File.Move(ffprobe, Path.Combine(targetDir, ffprobeName), overwrite: true);
        return true;
    }

    private static void MakeExecutable(string filePath)
    {
        var chmod = System.Diagnostics.Process.Start("chmod", $"+x \"{filePath}\"");
        chmod?.WaitForExit();
    }

    private static async Task<int> RunProcessAsync(string fileName, string arguments)
    {
        var psi = new System.Diagnostics.ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false
        };
        using var process = System.Diagnostics.Process.Start(psi)
            ?? throw new InvalidOperationException($"Не удалось запустить {fileName}");
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    private static void SetupPaths(string targetDir)
    {
        _ffmpegPath = targetDir;
        Environment.SetEnvironmentVariable("FFMPEG_PATH",  Path.Combine(targetDir, FfmpegExeName));
        Environment.SetEnvironmentVariable("FFPROBE_PATH", Path.Combine(targetDir, FfprobeExeName));
    }

    private static void Cleanup(string archivePath, string tempDir)
    {
        try
        {
            if (File.Exists(archivePath))      File.Delete(archivePath);
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
        catch { /* игнорируем */ }
    }

    private static void ShowFailureMessage(string targetDir)
    {
        ConsoleUI.WriteError("Критическая ошибка: FFmpeg не найден.");
        ConsoleUI.WriteWarning($"Скачайте бинарники вручную и положите в: {targetDir}");
    }

    public static string GetFFmpegPath() =>
        _ffmpegPath ?? throw new InvalidOperationException("FFmpeg не инициализирован");
}