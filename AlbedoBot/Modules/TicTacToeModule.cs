using AlbedoBot.Services;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace AlbedoBot.Modules
{
    [Group("ttt")]

    public sealed class TicTacToeModule : ModuleBase<SocketCommandContext>
    {
        public TicTacToeService TicTacToeService { get; set; }

        [Command("start")]
        [Alias("play")]
        [Summary("Starts a new game")]

        public async Task Start(IGuildUser firstPlayer, IGuildUser secondPlayer)
        {
            await ReplyAsync(await TicTacToeService.StartAsync(Context.Guild, firstPlayer, secondPlayer));
        }
    }
}