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

        [Command("turn")]
        [Summary("Makes a turn")]
        public async Task Turn(int position)
        {
            await ReplyAsync(await TicTacToeService.TurnAsync(Context.Guild, Context.User as IGuildUser, position - 1));
        }

        [Command("restart")]
        [Summary("Restart the current game")]
        public async Task Restart()
        {
            await ReplyAsync(await TicTacToeService.RestartAsync(Context.Guild));
        }

        [Command("end")]
        [Summary("Ends current game")]
        public async Task End()
        {
            await ReplyAsync(await TicTacToeService.EndAsync(Context.Guild));
        }
    }
}