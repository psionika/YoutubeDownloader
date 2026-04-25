using System.Diagnostics;
using System.Text;

namespace YoutubeDownloader;

class Program
{
    private readonly static Config _config = Config.Load();

    static void Main()
    {
        string[] videoLinks = ReadSourceFile();

        Console.WriteLine($"[Найдено ссылок для обработки] {videoLinks.Length}");
        Console.WriteLine("-------------------------------------------");

        List<string> failedUrls = ((Func<List<string>>)(() => DownloadAll(videoLinks))).Time("Общее время");

        if (failedUrls.Count != 0)
        {
            File.WriteAllLines(_config.ErrorLogFile, failedUrls);
            ConsoleWriter.Error($"[Ошибка] Завершено с ошибками ({failedUrls.Count}/{videoLinks.Length}). Список неудачных ссылок сохранен в: {_config.ErrorLogFile}");
        }
        else
        {
            ConsoleWriter.Success("Все ссылки обработаны успешно!");
        }
    }

    private static List<string> DownloadAll(string[] videoLinks)
    {
        List<string> failedUrls = [];

        for (int i = 0; i < videoLinks.Length; i++)
        {
            string link = videoLinks[i];

            ConsoleWriter.Warning($"[Обработка url] {link} ({i + 1}/{videoLinks.Length})");

            var success = ((Func<bool>)(() => DownloadVideo(link))).Time("Загрузка видео");

            if (success)
            {
                ConsoleWriter.Success("[Статус] Успешно");
            }
            else
            {
                ConsoleWriter.Error("[Статус] ОШИБКА");
                failedUrls.Add(link);
            }

            if (i + 1 != videoLinks.Length)
            {
                Console.WriteLine($"[Статус] Пауза {_config.PauseSeconds} секунд ({i + 1}/{videoLinks.Length})");
                Thread.Sleep(_config.PauseSeconds * 1000);
            }

            Console.WriteLine("-------------------------------------------");
        }

        return failedUrls;
    }

    private static string[] ReadSourceFile()
    {
        string[] urls = [.. File.ReadAllLines(_config.InputFile)
        .Select(line => line.Trim())
        .Where(line => line.StartsWith("http://") || line.StartsWith("https://"))];

        if (urls.Length == 0)
        {
            ConsoleWriter.Error($"[Ошибка] В файле {_config.InputFile} не найдено валидных ссылок (должны начинаться с http или https)");
            Environment.Exit(1);
        }

        return urls;
    }

    private static bool DownloadVideo(string url)
    {
        try
        {
            string[] args = [
                $"--cookies \"{_config.CookiePath}\"",
                $"--force-ipv4", // or "--force-ipv6 " for ipv6 connect
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

            ConsoleWriter.Info($"[Запускаем] {process.StartInfo.FileName}");
            foreach (var arg in args) ConsoleWriter.Info($"\t\t{arg}");

            process.Start();

            process.OutputDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) ConsoleWriter.Print(e.Data); };
            process.ErrorDataReceived += (_, e) =>  { if (!string.IsNullOrWhiteSpace(e.Data)) ConsoleWriter.Error(e.Data); };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            ConsoleWriter.Error($"[Ошибка] Исключение при запусе процесса: {ex.Message}");
            return false;
        }
    }
}