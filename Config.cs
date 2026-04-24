namespace YoutubeDownloader;

public class Config
{
    public string InputFile { get; set; } = string.Empty;
    public string ErrorLogFile { get; set; } = string.Empty;
    public int PauseSeconds { get; set; } = 30;

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
                case "pauseSeconds":
                    if (int.TryParse(value, out int seconds))
                        config.PauseSeconds = seconds;
                    else
                        ConsoleWriter.Warning($"[Предупреждение] Неверное значение '{key}': '{value}'. Установлено значение по умолчанию: 30 секунд");
                    break;
                case "ytDlpPath": config.YtDlpPath = value; break;
                case "cookiePath": config.CookiePath = value; break;
                case "ffmpegPath": config.FfmpegPath = value; break;
                case "ariaPath": config.AriaPath = value; break;
            }
        }

        // Валидация обязательных полей
        var errors = new List<string>();
        if (string.IsNullOrEmpty(config.InputFile)) errors.Add("inputFile");
        if (string.IsNullOrEmpty(config.ErrorLogFile)) errors.Add("errorLogFile");
        if (string.IsNullOrEmpty(config.YtDlpPath)) errors.Add("ytDlpPath");
        if (string.IsNullOrEmpty(config.CookiePath)) errors.Add("cookiePath");
        if (string.IsNullOrEmpty(config.FfmpegPath)) errors.Add("ffmpegPath");
        if (string.IsNullOrEmpty(config.AriaPath)) errors.Add("ariaPath");

        if (errors.Count > 0)
        {
            foreach (var missing in errors)
                ConsoleWriter.Error($"[Ошибка] Значение '{missing}' не найдено в config.txt");
            Environment.Exit(1);
        }

        return config;
    }
}