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
        string[] videoLinks = ReadSourceFile();

        List<string> failedUrls = [];

        Console.WriteLine($"Найдено ссылок для обработки: {videoLinks.Length}");
        Console.WriteLine("-------------------------------------------");

        for (int i = 0; i < videoLinks.Length; i++)
        {
            string link = videoLinks[i];

            ConsoleWriter.PrintWarning($"Обработка url: {link} ({i+1}/{videoLinks.Length})");

            bool success = DownloadVideo(link);

            if (success)
            {
                ConsoleWriter.PrintSuccess("Статус: Успешно");
            }
            else
            {
                ConsoleWriter.PrintError("Статус: ОШИБКА");
                failedUrls.Add(link);
            }

            if (i+1 != videoLinks.Length)
            {
                Console.WriteLine($"Статус: Пауза {PauseBetweenDownloadsSeconds} секунд"); 
                Thread.Sleep(PauseBetweenDownloadsSeconds * 1000);
            }
            
            Console.WriteLine("-------------------------------------------");
        }

        if (failedUrls.Count != 0)
        {
            File.WriteAllLines(ErrorLogFile, failedUrls);
            ConsoleWriter.PrintError($"Завершено с ошибками. Список неудачных ссылок сохранен в: {ErrorLogFile}");
        }
        else
        {
            ConsoleWriter.PrintSuccess("Все ссылки обработаны успешно!");
        }
    }

    private static string[] ReadSourceFile()
    {
        // Проверяем наличие входного файла
        if (!File.Exists(InputFile))
        {
            ConsoleWriter.PrintError($"Ошибка: Файл {InputFile} не найден!");
            Environment.Exit(1);
        }

        string[] urls = [.. File.ReadAllLines(InputFile).Where(line => !string.IsNullOrWhiteSpace(line))];

        // Проверяем количество строк в файле
        if (urls.Length < 1)
        {
            ConsoleWriter.PrintError($"Ошибка: Файл {InputFile} не должен быть пустым");
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

            ConsoleWriter.PrintInfo($"Запускаем: {process.StartInfo.FileName}");
            foreach (var arg in args) ConsoleWriter.PrintInfo($"\t\t{arg}");
            
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
            ConsoleWriter.PrintError($"Исключение при запусе процесса: {ex.Message}");
            return false;
        }
    }

    private static class ConsoleWriter
    {
        public static void PrintError(string message) => WriteLine(message, ConsoleColor.Red);
        public static void PrintSuccess(string message) => WriteLine(message, ConsoleColor.Green);
        public static void PrintWarning(string message) => WriteLine(message, ConsoleColor.Yellow);
        public static void PrintInfo(string message) => WriteLine(message, ConsoleColor.DarkMagenta);
        public static void Print(string message) => WriteLine(message, ConsoleColor.White);

        private static void WriteLine(string message, ConsoleColor? color)
        {
            if (color.HasValue)
                Console.ForegroundColor = color.Value;

            Console.WriteLine(message);

            if (color.HasValue)
                Console.ResetColor();
        }
    }
}