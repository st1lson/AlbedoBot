using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace AlbedoBot.Modules
{
    public sealed class TextModules : ModuleBase<SocketCommandContext>
    {
        [Command("hi")]

        public async Task Hello()
        {
            var user = Context.User;

            await ReplyAsync($"**Welcome back,** {user.Mention}");
        }

        [Command("roll")]

        public async Task Roll([Remainder] int upper = 100)
        {
            if (upper <= 0)
            {
                await ReplyAsync(":no_entry_sign: **Impossible operation**");
                return;
            }
            
            Random random = new Random();

            await ReplyAsync($"**You got** `{random.Next(upper)}/{upper}`");
        }

        [Command("flip")]

        public async Task Flip()
        {
            Random random = new Random();

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