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

        public static async Task<Embed> QueueEmbed(EmbedFieldBuilder[] fields)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithFields(fields).Build()));

            return embed;
        }

        public static async Task<EmbedFieldBuilder[]> AppendQueue(EmbedFieldBuilder[] fields, string title, string url, string duration, int position)
        {
            if (position == 0)
            {
                fields[position] = await Task.Run(() => (new EmbedFieldBuilder
                {
                    Name = "Now playing",
                    Value = $"**[{title}]({url})** || `Time left: {duration})`",
                    IsInline = false
                }));
            }
            else if (position == 1)
            {
                fields[position] = await Task.Run(() => (new EmbedFieldBuilder
                {
                    Name = "In queue",
                    Value = $"`{position}.` **[{title}]({url})** || `Time left: {duration}`",
                    IsInline = false
                }));
            }
            else if (position < fields.Length)
            {
                fields[position] = await Task.Run(() => (new EmbedFieldBuilder
                {
                    Name = $"`{position}.`",
                    Value = $"**[{title}]({url})** || `Time left: {duration}`",
                    IsInline = false
                }));
            }

            return fields;
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