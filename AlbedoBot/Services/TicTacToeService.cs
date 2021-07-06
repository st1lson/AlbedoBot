using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlbedoBot.Services
{
    public sealed class TicTacToeService
    {
        private readonly Dictionary<ulong, bool> _gameStarted;
        private readonly Dictionary<ulong, int> _turn;
        private readonly Dictionary<ulong, IGuildUser> _firstPlayer;
        private readonly Dictionary<ulong, IGuildUser> _secondPlayer;
        private readonly Dictionary<ulong, int[]> _playerTurns;

        public TicTacToeService()
        {
            _gameStarted = new Dictionary<ulong, bool>();
            _turn = new Dictionary<ulong, int>();
            _firstPlayer = new Dictionary<ulong, IGuildUser>();
            _secondPlayer = new Dictionary<ulong, IGuildUser>();
            _playerTurns = new Dictionary<ulong, int[]>();
        }

        public async Task<string> StartAsync(IGuild guild, IGuildUser firstPlayer, IGuildUser secondPlayer)
        {
            if (_gameStarted.TryGetValue(guild.Id, out var gameStarted) && gameStarted)
            {
                return "**:no_entry_sign: Game already started**";
            }

            _gameStarted.Add(guild.Id, true);
            _turn.Add(guild.Id, 0);
            _firstPlayer.Add(guild.Id, firstPlayer);
            _secondPlayer.Add(guild.Id, secondPlayer);
            _playerTurns.Add(guild.Id, new int[9]);
            await LogService.InfoAsync($"Tic-Tac-Toe started: {firstPlayer.Nickname} against {secondPlayer.Nickname}");
            return $"**Game started:\n`{firstPlayer.Nickname}` against `{secondPlayer.Nickname}`**";
        }
    }
}
