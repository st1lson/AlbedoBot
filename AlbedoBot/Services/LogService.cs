using System;
using System.Threading.Tasks;
using Discord;

namespace AlbedoBot.Services
{
    public static class LogService
    {
        public static async Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine($"[LOG]   {logMessage}");
        }

        public static async Task InfoAsync(string info)
        {
            Console.WriteLine($"[INFO]   {info}");
        }

        public static async Task ExceptionAsync(Exception exception)
        {
            Console.WriteLine($"[EXCEPTION]   {exception.Message}");
        }
    }
}
