using System.Text;

namespace YoutubeDownloader;

public class Config
{
    public string InputFile { get; set; } = string.Empty;
    public string ErrorLogFile { get; set; } = string.Empty;
    public int PauseSeconds { get; set; }

    public string YtDlpPath { get; set; } = string.Empty;
    public string CookiePath { get; set; } = string.Empty;
    public string FfmpegPath { get; set; } = string.Empty;
    public string AriaPath { get; set; } = string.Empty;

    public static Config Load(string path = "config.txt")
    {
        if (!File.Exists(path))
        {
            ConsoleWriter.Error($"[Ошибка] Файл конфигурации '{path}' не найден!");
            Environment.Exit(1);
        }

        var config = new Config();

        foreach (var line in File.ReadLines(path))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            switch (key)
            {
                case "inputFile": config.InputFile = value; break;
                case "errorLogFile": config.ErrorLogFile = value; break;
                case "pauseSeconds": config.PauseSeconds = int.Parse(value); break;
                case "ytDlpPath": config.YtDlpPath = value; break;
                case "cookiePath": config.CookiePath = value; break;
                case "ffmpegPath": config.FfmpegPath = value; break;
                case "ariaPath": config.AriaPath = value; break;
            }
        }

        return config;
    }
}