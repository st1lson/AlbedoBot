using System.Threading.Tasks;
using AlbedoBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace AlbedoBot.Modules
{
    public sealed class MusicModule : ModuleBase<SocketCommandContext>
    {
        public MusicService MusicService { get; set; }

        [Command("join")]

        public async Task Join()
        {
            var user = Context.User as SocketGuildUser;

            await ReplyAsync(await MusicService.JoinAsync(Context.Guild, user.VoiceChannel,
                Context.Channel as ITextChannel));
        }

        [Command("play")]

        public async Task Play([Remainder] string trackTitle)
        {
            var user = Context.User as SocketGuildUser;

            if (!MusicService.Joined(user.Guild))
            {
                string joinResult = await MusicService.JoinAsync(Context.Guild, user.VoiceChannel, Context.Channel as ITextChannel, 1);

                await ReplyAsync(joinResult);

                if (joinResult.Equals(":no_entry_sign: **You need to join to a voice channel!**")
                    || joinResult.Equals(":no_entry_sign: **I'm already connected to a voice channel!**"))
                {
                    return;
                }
            }

            await ReplyAsync($"**Searching** :mag: `{trackTitle}`");

            await ReplyAsync(embed: await MusicService.PlayAsync(user, Context.Guild, trackTitle));
        }

        [Command("skip")]

        public async Task Skip()
        {
            await ReplyAsync(await MusicService.SkipAsync(Context.Guild));
        }

        [Command("leave")]

        public async Task Leave()
        {
            await ReplyAsync(await MusicService.LeaveAsync(Context.Guild));
        }


    }
}