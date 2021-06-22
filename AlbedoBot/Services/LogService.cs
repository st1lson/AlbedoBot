using System;
using System.Threading.Tasks;


namespace AlbedoBot.Services
{
    public static class LogService
    {
        public static async Task LogAsync(string logMessage, string state = "log")
        {
            await Write($"{Tag(state)} {logMessage}");
        }

        public static async Task InfoAsync(string info)
        {
            await LogAsync(info, "info");
        }

        public static async Task ExceptionAsync(Exception exception)
        {
            await LogAsync(exception.Message, "exception");
        }

        private static Task Write(string info)
        {
            Console.WriteLine(info);

            return Task.CompletedTask;
        }

        private static string Tag(string sender)
        {
            switch (sender)
            {
                case "log":
                    return "[LOGM]";
                case "info":
                    return "[INFO]";
                case "exception":
                    return "[EXCP]";
            }

            return "[EROR]";
        }
    }
}