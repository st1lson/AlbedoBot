using Discord;
using System;
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
        private readonly int[][] _winState;

        public TicTacToeService()
        {
            _gameStarted = new Dictionary<ulong, bool>();
            _turn = new Dictionary<ulong, int>();
            _firstPlayer = new Dictionary<ulong, IGuildUser>();
            _secondPlayer = new Dictionary<ulong, IGuildUser>();
            _playerTurns = new Dictionary<ulong, int[]>();
            _winState = InitializeWinState();
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

        public async Task<string> TurnAsync(IGuild guild, IGuildUser sender, int position)
        {
            if (!_gameStarted.TryGetValue(guild.Id, out var gameStarted) || !gameStarted)
            {
                return "**:no_entry_sign: Game did not started**";
            }

            if (_turn.TryGetValue(guild.Id, out var turn))
            {
                if (turn % 2 == 0)
                {
                    if (_firstPlayer.TryGetValue(guild.Id, out var firstPlayer) && !firstPlayer.Equals(sender))
                    {
                        return "**Not your turn**";
                    }

                    if (_playerTurns.TryGetValue(guild.Id, out var playerTurns) && playerTurns[position] == 0)
                    {
                        playerTurns[position] = 1;
                        _turn[guild.Id]++;
                        await LogService.InfoAsync($"Turn [{position}] was successfull.");
                        var table = await CreateTable(guild);
                        if (CheckForWin(guild))
                        {
                            await LogService.InfoAsync($"Winner is {firstPlayer.Nickname}");
                            return $"{table}\n**Winner is **{firstPlayer.Mention}";
                        }

                        if (_turn[guild.Id] == 9)
                        {
                            await EndAsync(guild);
                            return $"{table}\n**Draw**";
                        }

                        return table;
                    }

                    await LogService.InfoAsync("This position already captured");
                    return "**This position already captured**";
                }
                else
                {
                    if (_secondPlayer.TryGetValue(guild.Id, out var secondPlayer) && !secondPlayer.Equals(sender))
                    {
                        return "**Not your turn**";
                    }

                    if (_playerTurns.TryGetValue(guild.Id, out var playerTurns) && playerTurns[position] == 0)
                    {
                        playerTurns[position] = 2;
                        _turn[guild.Id]++;
                        await LogService.InfoAsync($"Turn [{position}] was successfull.");
                        var table = await CreateTable(guild);
                        if (CheckForWin(guild))
                        {
                            await LogService.InfoAsync($"Winner is {secondPlayer.Nickname}");
                            return $"{table}**Winner is **{secondPlayer.Mention}";
                        }

                        if (_turn[guild.Id] == 9)
                        {
                            await EndAsync(guild);
                            return $"{table}\n**Draw**";
                        }

                        return table;
                    }

                    await LogService.InfoAsync("This position already captured");
                    return "**This position already captured**";
                }
            }

            return ":no_entry_sign: **Something going wrong.**";
        }

        public async Task<string> RestartAsync(IGuild guild)
        {
            if (!_gameStarted.TryGetValue(guild.Id, out var gameStarted) || !gameStarted)
            {
                return "**:no_entry_sign: Game did not started**";
            }

            _turn[guild.Id] = 0;
            _playerTurns[guild.Id] = new int[9];

            await LogService.InfoAsync("TicTacToe game restarted");
            return ":ballot_box_with_check: **Game successfully restarted**";
        }

        public async Task<string> EndAsync(IGuild guild)
        {
            if (!_gameStarted.TryGetValue(guild.Id, out var gameStarted) || !gameStarted)
            {
                return "**:no_entry_sign: Game did not started**";
            }

            _gameStarted.Remove(guild.Id);
            _turn.Remove(guild.Id);
            _firstPlayer.Remove(guild.Id);
            _secondPlayer.Remove(guild.Id);
            _playerTurns.Remove(guild.Id);

            await LogService.InfoAsync("TicTacToe game ended");
            return ":ballot_box_with_check: **Game successfully ended**";
        }

        private async Task<string> CreateTable(IGuild guild)
        {
            string table = "|";

            if (_playerTurns.TryGetValue(guild.Id, out var playerTurns))
            {
                int n = 0;
                for (int i = 0; i < playerTurns.GetLength(0); i++)
                {
                    string value = String.Empty;
                    if (playerTurns[i] == 1)
                    {
                        value = ":x:";
                    }
                    else if (playerTurns[i] == 2)
                    {
                        value = ":o:";
                    }
                    else
                    {
                        value = "      ";
                    }

                    if (n % 3 == 0 && i != 0)
                    {
                        table += "|";
                    }
                    table += $" {value} |";
                    n++;
                    if (n % 3 == 0)
                    {
                        table += "\n";
                    }
                }
            }

            await LogService.InfoAsync("TicTacToe table created");
            return table;
        }

        private bool CheckForWin(IGuild guild)
        {
            if (_playerTurns.TryGetValue(guild.Id, out var playerTurns))
            {
                foreach (var winState in _winState)
                {
                    int first = 0;
                    int second = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (playerTurns[winState[i]] == 0)
                        {
                            continue;
                        }
                        else if (playerTurns[winState[i]] == 1)
                        {
                            first++;
                        }
                        else if (playerTurns[winState[i]] == 2)
                        {
                            second++;
                        }
                    }

                    if (first == 3 || second == 3)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private int[][] InitializeWinState()
        {
            int[][] winState = new int[8][];
            winState[0] = new int[] { 0, 1, 2 };
            winState[1] = new int[] { 3, 4, 5 };
            winState[2] = new int[] { 6, 7, 8 };
            winState[3] = new int[] { 0, 3, 6 };
            winState[4] = new int[] { 1, 4, 7 };
            winState[5] = new int[] { 2, 5, 8 };
            winState[6] = new int[] { 0, 4, 8 };
            winState[7] = new int[] { 2, 4, 6 };

            return winState;
        }
    }
}
