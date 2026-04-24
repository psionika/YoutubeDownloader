using System.Diagnostics;
using System.Text;

class Program
{
    #region Constants

    private const string InputFile = "youtube_links.txt"; // Файл с исходными ссылками
    private const string ErrorLogFile = "failed_links.txt"; // Файл для ошибок
    private const int PauseBetweenDownloadsSeconds = 30; // Пауза между попытками скачивания в секундах

    // Link: https://github.com/yt-dlp/yt-dlp
    private const string YtDlpFile = @"C:\Downloads\yt-dlp\yt-dlp.exe";

    // Link: https://github.com/yt-dlp/yt-dlp/wiki/FAQ#how-do-i-pass-cookies-to-yt-dlp
    private const string CookieFile = @"C:\Downloads\yt-dlp\cookies.txt";

    // Link: https://github.com/yt-dlp/FFmpeg-Builds
    private const string FfmpegFile = @"C:\Downloads\yt-dlp\ffmpeg\bin\ffmpeg.exe";

    // Link: https://github.com/aria2/aria2
    private const string AriaFile = @"C:\Downloads\yt-dlp\aria2c.exe";

    #endregion Constants

    static void Main()
    {
        CheckRequiredFiles();

        string[] videoLinks = ReadSourceFile();

        List<string> failedUrls = [];

        Console.WriteLine($"[Найдено ссылок для обработки] {videoLinks.Length}");
        Console.WriteLine("-------------------------------------------");

        for (int i = 0; i < videoLinks.Length; i++)
        {
            string link = videoLinks[i];

            ConsoleWriter.Warning($"[Обработка url] {link} ({i+1}/{videoLinks.Length})");

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

            if (i+1 != videoLinks.Length)
            {
                Console.WriteLine($"[Статус] Пауза {PauseBetweenDownloadsSeconds} секунд"); 
                Thread.Sleep(PauseBetweenDownloadsSeconds * 1000);
            }
            
            Console.WriteLine("-------------------------------------------");
        }

        if (failedUrls.Count != 0)
        {
            File.WriteAllLines(ErrorLogFile, failedUrls);
            ConsoleWriter.Error($"[Ошибка] Завершено с ошибками. Список неудачных ссылок сохранен в: {ErrorLogFile}");
        }
        else
        {
            ConsoleWriter.Success("Все ссылки обработаны успешно!");
        }
    }

    private static void CheckRequiredFiles()
    {
        string[] requiredFiles = [InputFile, YtDlpFile, CookieFile, FfmpegFile, AriaFile];

        foreach (var file in requiredFiles)
        {
            if (!File.Exists(file))
            {
                ConsoleWriter.Error($"[Ошибка] Файл {file} не найден!");
                Environment.Exit(1);
            }
        }
    }

    private static string[] ReadSourceFile()
    {
        string[] urls = [.. File.ReadAllLines(InputFile)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("http://") || line.StartsWith("https://"))];

        if (urls.Length == 0)
        {
            ConsoleWriter.Error($"[Ошибка] В файле {InputFile} не найдено валидных ссылок (должны начинаться с http или https)");
            Environment.Exit(1);
        }

        return urls;
    }

    private static bool DownloadVideo(string url)
    {
        try
        {
            string[] args = [
                $"--cookies \"{CookieFile}\" ",
                $"--force-ipv4 ", // or "--force-ipv6 " for ipv6 connect
                $"--ffmpeg-location \"{FfmpegFile}\" ",
                $"--external-downloader \"{AriaFile}\" ",
                $"--encoding utf-8 ",
                $"\"{url}\"" ];

            // Настройка процесса для запуска yt-dlp
            ProcessStartInfo startInfo = new()
            {
                FileName = YtDlpFile,
                Arguments = string.Join("", args),
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardError = true, 
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using Process process = new() { StartInfo = startInfo };

            ConsoleWriter.Info($"[Запускаем] {process.StartInfo.FileName}");
            foreach (var arg in args) ConsoleWriter.Info($"\t\t{arg}");
            
            process.Start();

            process.OutputDataReceived += (o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Console.WriteLine(e.Data);
                }
            };
            process.BeginOutputReadLine();

            string err = process.StandardError.ReadToEnd();
            Console.WriteLine(err);

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

public static class ConsoleWriter
{
    public static void Error(string msg) => Write(msg, ConsoleColor.Red);
    public static void Success(string msg) => Write(msg, ConsoleColor.Green);
    public static void Warning(string msg) => Write(msg, ConsoleColor.Yellow);
    public static void Info(string msg) => Write(msg, ConsoleColor.DarkMagenta);
    public static void Print(string msg) => Write(msg, ConsoleColor.White);

    static void Write(string msg, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
        Console.ResetColor();
    }
}

public static class TimingExtensions
{
    static string Format(TimeSpan elapsed)
    {
        var parts = new List<string>();

        if (elapsed.Hours > 0)
            parts.Add($"{elapsed.Hours:D2} ч.");

        if (elapsed.Minutes > 0 || parts.Count > 0)
            parts.Add($"{elapsed.Minutes:D2} мин.");

        parts.Add($"{elapsed.Seconds:D2} сек.");

        return string.Join(" ", parts);
    }

    public static T Time<T>(this Func<T> action, string label = "")
    {
        var sw = Stopwatch.StartNew();
        var result = action();
        sw.Stop();

        ConsoleWriter.Warning($"[{label}] Выполнено за {Format(sw.Elapsed)}");
        return result;
    }
}