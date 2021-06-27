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
        [Summary("Command to join bot to your voice channel")]

        public async Task Join()
        {
            var user = Context.User as SocketGuildUser;

            if (user is null) return;
            
            await ReplyAsync(await MusicService.JoinAsync(Context.Guild, user.VoiceChannel, Context.Channel as ITextChannel));
        }

        [Command("play")]
        [Alias("p", "track")]
        [Summary("Command to play a track")]

        public async Task Play([Remainder] string trackTitle)
        {
            var user = Context.User as SocketGuildUser;

            if (user is not null && !MusicService.Joined(user.Guild))
            {
                var joinResult = await MusicService.JoinAsync(Context.Guild, user.VoiceChannel, Context.Channel as ITextChannel);

                await ReplyAsync(joinResult);

                if (joinResult.Equals(":no_entry_sign: **You need to join to a voice channel!**") || joinResult.Equals(":no_entry_sign: **I'm already connected to a voice channel!**"))
                {
                    return;
                }
            }

            await ReplyAsync($"**Searching** :mag: `{trackTitle}`");

            await ReplyAsync(embed: await MusicService.PlayAsync(user, Context.Guild, trackTitle));
        }

        [Command("pause")]
        [Alias("stop")]
        [Summary("Command to pause current track")]

        public async Task Pause()
        {
            await ReplyAsync(await MusicService.PauseAsync(Context.Guild));
        }

        [Command("resume")]
        [Alias("continue")]
        [Summary("Command to resume paused track")]

        public async Task Resume()
        {
            await ReplyAsync(await MusicService.ResumeAsync(Context.Guild));
        }
        
        [Command("skip")]
        [Alias("s", "next")]
        [Summary("Command to skip current track")]

        public async Task Skip()
        {
            await ReplyAsync(await MusicService.SkipAsync(Context.Guild));
        }

        [Command("leave")]
        [Summary("Command to force the bot to leave current voice channel")]

        public async Task Leave()
        {
            await ReplyAsync(await MusicService.LeaveAsync(Context.Guild));
        }

        [Command("queue")]
        [Alias("tracks", "list")]
        [Summary("Command to check the queue")]

        public async Task Queue()
        {
            await ReplyAsync(embed: await MusicService.QueueAsync(Context.Guild));
        }
        
        [Command("left")]
        [Alias("time")]
        [Summary("Command to check the time to the end of the current track")]

        public async Task Left()
        {
            await ReplyAsync(await MusicService.LeftAsync(Context.Guild));
        }

        [Command("now")]
        [Alias("current", "playing")]
        [Summary("Command to check the current track information")]

        public async Task Now()
        {
            await ReplyAsync(embed: await MusicService.NowAsync(Context.Guild));
        }

        [Command("repeat")]
        [Alias("loop")]
        [Summary("Command to repeat the current track")]

        public async Task Repeat()
        {
            await ReplyAsync(await MusicService.RepeatAsync(Context.Guild));
        }


        [Command("clear")]
        [Summary("Command to clear the queue")]

        public async Task Clear()
        {
            await ReplyAsync(await MusicService.ClearAsync(Context.Guild));
        }
    }
}
