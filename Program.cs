using System.Diagnostics;
using System.Text;

namespace YoutubeDownloader;

class Program
{
    private readonly static Config _config = Config.Load();
    private static CancellationTokenSource? _cts;

    static async Task Main()
    {
        _cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            ConsoleWriter.Warning("\n[Ctrl+C] Interrupting... Stopping after current operation completes.");
            _cts?.Cancel();
        };

        string[] videoLinks = ReadSourceFile();

        Console.WriteLine($"[Found links to process] {videoLinks.Length}");
        Console.WriteLine("-------------------------------------------");

        try
        {
            List<string> failedUrls = await ((Func<Task<List<string>>>)(() => DownloadAll(videoLinks, _cts.Token))).TimeAsync("Total time");

            if (failedUrls.Count != 0)
            {
                File.AppendAllLines(_config.ErrorLogFile, failedUrls);
                ConsoleWriter.Error($"[Error] Completed with errors ({failedUrls.Count}/{videoLinks.Length}). List of failed links saved to: {_config.ErrorLogFile}");
            }
            else
            {
                ConsoleWriter.Success("All links processed successfully!");
            }
        }
        catch (OperationCanceledException)
        {
            ConsoleWriter.Error("[Error] Process was interrupted by user (Ctrl+C).");
        }
        catch (Exception ex)
        {
            ConsoleWriter.Error("[Error] An exception occurred: " + ex.Message);
        }
    }

    private static async Task<List<string>> DownloadAll(string[] videoLinks, CancellationToken ct)
    {
        List<string> failedUrls = [];

        for (int i = 0; i < videoLinks.Length; i++)
        {
            string link = videoLinks[i];

            ConsoleWriter.Important($"[Processing URL] {link} ({i + 1}/{videoLinks.Length})");

            var success = await ((Func<Task<bool>>)(() => DownloadVideo(link, ct))).TimeAsync("Video download");

            if (success)
            {
                ConsoleWriter.Success("[Status] Success");
            }
            else
            {
                ConsoleWriter.Error("[Status] ERROR");
                failedUrls.Add(link);
            }

            if (i + 1 != videoLinks.Length)
            {
                Console.WriteLine($"[Status] Pausing for {_config.PauseSeconds} seconds ({i + 1}/{videoLinks.Length})");
                await Task.Delay(_config.PauseSeconds * 1000, ct);
            }

            Console.WriteLine("-------------------------------------------");
        }

        return failedUrls;
    }

    private static string[] ReadSourceFile()
    {
        string[] urls = [.. File.ReadAllLines(_config.InputFile)
        .Select(line => line.Trim())
        .Where(line => line.StartsWith("http://") || line.StartsWith("https://"))
        .Distinct(StringComparer.OrdinalIgnoreCase)];

        if (urls.Length == 0)
        {
            ConsoleWriter.Error($"[Error] No valid links found in {_config.InputFile} (links must start with http or https)");
            Environment.Exit(1);
        }

        return urls;
    }

    private static async Task<bool> DownloadVideo(string url, CancellationToken ct)
    {
        try
        {
            string[] args = [
                $"--cookies \"{_config.CookiePath}\"",
                $"--force-ipv4",
                $"--ffmpeg-location \"{_config.FfmpegPath}\"",
                $"--external-downloader \"{_config.AriaPath}\"",
                $"--encoding utf-8",
                $"\"{url}\"" ];

            ProcessStartInfo startInfo = new()
            {
                FileName = _config.YtDlpPath,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using Process process = new() { StartInfo = startInfo };

            ConsoleWriter.Info($"[Launching] {process.StartInfo.FileName}");
            foreach (var arg in args) ConsoleWriter.Info($"\t\t{arg}");

            process.OutputDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) ConsoleWriter.Print(e.Data); };
            process.ErrorDataReceived += (_, e) =>  { if (!string.IsNullOrWhiteSpace(e.Data)) ConsoleWriter.Error(e.Data); };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            return process.ExitCode == 0;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ConsoleWriter.Error($"[Error] Exception while launching process: {ex.Message}");
            return false;
        }
    }
}
