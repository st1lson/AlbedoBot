using System;
using System.Threading.Tasks;
using Discord;

namespace AlbedoBot.Services
{
    public static class EmbedService
    {
        public static async Task<Embed> Embed(string action, string title, string url, int position, string duration, string timeLeft, Color color)
        {
            if (timeLeft.Equals(TimeSpan.Zero.ToString()))
            {
                timeLeft = "Now";
            }

            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(action)
                .WithDescription($"[**{title}**]({url})")
                .WithColor(color)
                .AddField("Position in queue", position, true)
                .AddField("Duration", duration, true)
                .AddField("Until play", timeLeft, true)
                .WithFooter(new EmbedFooterBuilder().Text = "Albedo bot").Build()));

            return embed;
        }

        public static async Task<Embed> ErrorEmbed(string title, string exception, Color color)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(exception)
                .WithColor(color)
                .WithFooter(new EmbedFooterBuilder().Text = "Albedo bot").Build()));

            return embed;
        }
    }
}