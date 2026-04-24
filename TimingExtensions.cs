using System.Diagnostics;

namespace YoutubeDownloader
{
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
}
