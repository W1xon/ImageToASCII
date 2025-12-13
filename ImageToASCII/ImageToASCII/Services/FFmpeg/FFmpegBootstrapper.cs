using System.Diagnostics;
using System.Net;
using ImageToASCII.UI;
using Xabe.FFmpeg.Downloader;

namespace ImageToASCII.Services;
public static class FFmpegBootstrapper
{
    private static bool _isReady;
    private static string? _ffmpegPath;
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    public static async Task<bool> EnsureFFmpegAsync(string targetDir)
    {
        if (_isReady)
            return true;

        Directory.CreateDirectory(targetDir);

        try
        {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls;
        }
        catch { }

        // Стратегия 1: Проверяем системный PATH
        if (await HasFfmpegInPathAsync())
        {
            _isReady = true;
            return true;
        }

        // Стратегия 2: Проверяем локальные файлы
        if (HasLocalFFmpegFiles(targetDir))
        {
            SetFFmpegPath(targetDir);
            return _isReady = true;
        }

        // Стратегия 3: Пытаемся скачать с retry
        ConsoleUI.ShowProgress("Загрузка FFmpeg...");
        if (await TryDownloadFFmpegWithRetryAsync(targetDir))
        {
            SetFFmpegPath(targetDir);
            return _isReady = true;
        }

        // Стратегия 4: Альтернативный источник
        if (await TryDownloadFromAlternativeSourceAsync(targetDir))
        {
            SetFFmpegPath(targetDir);
            return _isReady = true;
        }

        ConsoleUI.WriteError("Не удалось получить FFmpeg");
        ConsoleUI.WriteWarning("Скачайте вручную: https://ffmpeg.org/download.html");
        ConsoleUI.WriteWarning($"Поместите ffmpeg.exe и ffprobe.exe в: {targetDir}");
        
        return _isReady = false;
    }

    public static string GetFFmpegPath()
    {
        if (!_isReady || string.IsNullOrEmpty(_ffmpegPath))
            throw new InvalidOperationException("FFmpeg not initialized");
        return _ffmpegPath;
    }

    private static void SetFFmpegPath(string targetDir)
    {
        string ffmpegExe = Path.Combine(targetDir, "ffmpeg.exe");
        string ffprobeExe = Path.Combine(targetDir, "ffprobe.exe");
        
        if (!File.Exists(ffmpegExe) || !File.Exists(ffprobeExe))
        {
            throw new FileNotFoundException($"FFmpeg files not found in {targetDir}");
        }
        
        _ffmpegPath = targetDir;
        Xabe.FFmpeg.FFmpeg.SetExecutablesPath(targetDir);
        
        Environment.SetEnvironmentVariable("FFMPEG_PATH", ffmpegExe);
        Environment.SetEnvironmentVariable("FFPROBE_PATH", ffprobeExe);
    }

    private static bool HasLocalFFmpegFiles(string targetDir)
    {
        string ffmpegPath = Path.Combine(targetDir, "ffmpeg.exe");
        string ffprobePath = Path.Combine(targetDir, "ffprobe.exe");
        
        if (File.Exists(ffmpegPath) && File.Exists(ffprobePath))
            return true;

        var dirs = Directory.GetDirectories(targetDir, "*", SearchOption.AllDirectories);
        foreach (var dir in dirs)
        {
            ffmpegPath = Path.Combine(dir, "ffmpeg.exe");
            ffprobePath = Path.Combine(dir, "ffprobe.exe");
            
            if (File.Exists(ffmpegPath) && File.Exists(ffprobePath))
            {
                File.Copy(ffmpegPath, Path.Combine(targetDir, "ffmpeg.exe"), true);
                File.Copy(ffprobePath, Path.Combine(targetDir, "ffprobe.exe"), true);
                return true;
            }
        }
        
        return false;
    }

    private static async Task<bool> TryDownloadFFmpegWithRetryAsync(string targetDir, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, targetDir);
                
                if (!VerifyFFmpegFiles(targetDir))
                {
                    if (FindAndCopyFFmpegFiles(targetDir))
                        return true;
                    
                    throw new FileNotFoundException("FFmpeg files not found after download");
                }
                
                return true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                }
            }
            catch (Exception)
            {
                if (attempt == maxRetries)
                    return false;
            }
        }
        
        return false;
    }

    private static bool VerifyFFmpegFiles(string targetDir)
    {
        string ffmpegPath = Path.Combine(targetDir, "ffmpeg.exe");
        string ffprobePath = Path.Combine(targetDir, "ffprobe.exe");
        return File.Exists(ffmpegPath) && File.Exists(ffprobePath);
    }

    private static bool FindAndCopyFFmpegFiles(string targetDir)
    {
        try
        {
            var ffmpegFiles = Directory.GetFiles(targetDir, "ffmpeg.exe", SearchOption.AllDirectories);
            var ffprobeFiles = Directory.GetFiles(targetDir, "ffprobe.exe", SearchOption.AllDirectories);
            
            if (ffmpegFiles.Length > 0 && ffprobeFiles.Length > 0)
            {
                string targetFfmpeg = Path.Combine(targetDir, "ffmpeg.exe");
                string targetFfprobe = Path.Combine(targetDir, "ffprobe.exe");
                
                File.Copy(ffmpegFiles[0], targetFfmpeg, overwrite: true);
                File.Copy(ffprobeFiles[0], targetFfprobe, overwrite: true);
                
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> TryDownloadFromAlternativeSourceAsync(string targetDir)
    {
        try
        {
            string version = "6.1";
            string downloadUrl = $"https://github.com/GyanD/codexffmpeg/releases/download/{version}/ffmpeg-{version}-essentials_build.zip";
            string zipPath = Path.Combine(targetDir, "ffmpeg.zip");
            
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return false;

            await using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);
            
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, targetDir, overwriteFiles: true);
            
            var ffmpegFiles = Directory.GetFiles(targetDir, "ffmpeg.exe", SearchOption.AllDirectories);
            var ffprobeFiles = Directory.GetFiles(targetDir, "ffprobe.exe", SearchOption.AllDirectories);
            
            if (ffmpegFiles.Length > 0 && ffprobeFiles.Length > 0)
            {
                File.Move(ffmpegFiles[0], Path.Combine(targetDir, "ffmpeg.exe"), overwrite: true);
                File.Move(ffprobeFiles[0], Path.Combine(targetDir, "ffprobe.exe"), overwrite: true);
                File.Delete(zipPath);
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> HasFfmpegInPathAsync()
    {
        try
        {
            var p = Process.Start(new ProcessStartInfo("ffmpeg", "-version")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            
            if (p == null) 
                return false;
            
            await p.WaitForExitAsync();
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

}