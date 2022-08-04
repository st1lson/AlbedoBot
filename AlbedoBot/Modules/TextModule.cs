using Discord.Commands;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace AlbedoBot.Modules
{
    public sealed class TextModules : ModuleBase<SocketCommandContext>
    {
        [Command("hi")]
        [Alias("hello")]
        [Summary("Simple hello command")]
        public async Task Hello()
        {
            SocketUser user = Context.User;
            await ReplyAsync($"**Welcome back,** {user.Mention}");
        }

        [Command("roll")]
        [Alias("spin")]
        [Summary("Command to get a random value in a selected range")]
        public async Task Roll([Remainder] int upper = 100)
        {
            if (upper <= 0)
            {
                await ReplyAsync(":no_entry_sign: **Impossible operation**");
                return;
            }

            Random random = new();
            await ReplyAsync($"**You got** `{random.Next(upper)}/{upper}`");
        }

        [Command("flip")]
        [Alias("coin")]
        [Summary("Command to get a random side of the coin")]
        public async Task Flip()
        {
            Random random = new();
            int flip = random.Next(2);
            if (flip == 0)
            {
                await ReplyAsync("**Heads**");
            }
            else
            {
                await ReplyAsync("**Tails**");
            }
        }
    }
}