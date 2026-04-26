namespace YoutubeDownloader;

public static class ConsoleWriter
{
    public static void Error(string msg) => Write(msg, ConsoleColor.Red);
    public static void Success(string msg) => Write(msg, ConsoleColor.Green);
    public static void Warning(string msg) => Write(msg, ConsoleColor.Yellow);
    public static void Important(string msg) => Write(msg, ConsoleColor.Cyan);
    public static void Info(string msg) => Write(msg, ConsoleColor.Magenta);
    public static void Print(string msg) => Write(msg, ConsoleColor.White);

    static void Write(string msg, ConsoleColor color)
    {
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
        }
        finally
        {
            Console.ResetColor();
        }
    }
}
