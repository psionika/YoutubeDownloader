namespace YoutubeDownloader
{
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
}