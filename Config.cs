using System.Reflection;

namespace YoutubeDownloader;

public class Config
{
    [FileRequired]
    public string InputFile { get; set; } = string.Empty;

    public string ErrorLogFile { get; set; } = string.Empty;

    public int PauseSeconds { get; set; } = 30;

    [FileRequired]
    public string YtDlpPath { get; set; } = string.Empty;

    [FileRequired]
    public string CookiePath { get; set; } = string.Empty;

    [FileRequired]
    public string FfmpegPath { get; set; } = string.Empty;

    [FileRequired]
    public string AriaPath { get; set; } = string.Empty;

    static void ValidateConfig(Config config)
    {
        var fileProps = typeof(Config)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<FileRequiredAttribute>() is not null);

        // 1) Check that required fields are set
        var missing = fileProps
            .Where(p => string.IsNullOrEmpty((string)p.GetValue(config)!))
            .Select(p => p.Name)
            .ToList();

        if (missing.Count > 0)
        {
            foreach (var key in missing)
                ConsoleWriter.Error($"[Error] Value for '{key}' not found in config.txt");
            Environment.Exit(1);
        }

        // 2) Check that files exist
        var missingFiles = fileProps
            .Where(p => !File.Exists((string)p.GetValue(config)!))
            .Select(p => $"{p.Name}: {(string)p.GetValue(config)!}")
            .ToList();

        if (missingFiles.Count > 0)
        {
            foreach (var file in missingFiles)
                ConsoleWriter.Error($"[Error] File not found: {file}");
            Environment.Exit(1);
        }
    }

    public static Config Load(string configPath = "config.txt")
    {
        if (!File.Exists(configPath))
        {
            ConsoleWriter.Error($"[Error] Config file '{configPath}' not found!");
            Environment.Exit(1);
        }

        var config = new Config();
        var props = typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var line in File.ReadLines(configPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            var prop = props.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
            {
                ConsoleWriter.Warning($"[Warning] Unknown key '{key}' in config.txt");
                continue;
            }

            try
            {
                prop.SetValue(config, Convert.ChangeType(value, prop.PropertyType));
            }
            catch (Exception ex)
            {
                ConsoleWriter.Warning($"[Warning] Invalid value for '{key}': {ex.Message}");
            }
        }

        ValidateConfig(config);

        return config;
    }
}