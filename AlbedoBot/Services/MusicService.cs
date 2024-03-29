using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace AlbedoBot.Services
{
    public sealed class MusicService
    {
        private readonly LavaNode _lavaNode;
        private readonly Dictionary<ulong, TimeSpan> _timeLeft;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private readonly ConcurrentDictionary<ulong, bool> _repeatTokens;

        public MusicService(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            _timeLeft = new Dictionary<ulong, TimeSpan>();
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            _repeatTokens = new ConcurrentDictionary<ulong, bool>();
        }

        public bool Joined(IGuild guild) => _lavaNode.HasPlayer(guild);

        public async Task<string> JoinAsync(IGuild guild, SocketVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild))
            {
                return ":no_entry_sign: **I'm already connected to a voice channel!**";
            }

            if (voiceChannel is null)
            {
                return ":no_entry_sign: **You need to join to a voice channel!**";
            }

            try
            {
                await LogService.InfoAsync("Joined");

                await _lavaNode.JoinAsync(voiceChannel, textChannel);
                return $":ballot_box_with_check: **Joined** `{voiceChannel.Name}`.";
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);

                return exception.Message;
            }
        }

        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string trackTitle)
        {
            if (user.VoiceChannel is null)
            {
                return await EmbedService.ErrorEmbed("No connection", "You need to join to a voice channel!", Color.DarkBlue);
            }

            if (!_lavaNode.HasPlayer(guild))
            {
                return await EmbedService.ErrorEmbed("No connection", "I'm not connected to a voice channel", Color.DarkBlue);
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                SearchResponse results = await _lavaNode.SearchYouTubeAsync(trackTitle);
                if (results.Status.Equals(SearchStatus.LoadFailed) || results.Status.Equals(SearchStatus.NoMatches))
                {
                    return await EmbedService.ErrorEmbed("No matches found", $"No matches were found for `{trackTitle}`", Color.DarkPurple);
                }

                LavaTrack track = results.Tracks.FirstOrDefault();
                if (track is null)
                {
                    return await EmbedService.ErrorEmbed("No matches found", $"No matches were found for `{trackTitle}`", Color.DarkPurple);
                }

                if (player.PlayerState is PlayerState.Playing)
                {
                    player.Queue.Enqueue(track);
                    if (_timeLeft.TryGetValue(player.VoiceChannel.Id, out TimeSpan timeLeft))
                    {
                        _timeLeft[player.VoiceChannel.Id] = timeLeft + track.Duration;
                    }

                    return await EmbedService.Embed("Added to the queue", track.Title, track.Url, YouTubeService.GetThumbnail(track.Url), player.Queue.Count, $"{track.Duration:hh\\:mm\\:ss}", $"{(timeLeft - player.Track.Position):hh\\:mm\\:ss}", Color.Green);
                }
                else
                {
                    await player.PlayAsync(track);
                    if (!_timeLeft.TryGetValue(player.VoiceChannel.Id, out TimeSpan timeLeft))
                    {
                        timeLeft = track.Duration;
                        _timeLeft.TryAdd(player.VoiceChannel.Id, timeLeft);
                    }
                    else
                    {
                        _timeLeft[player.VoiceChannel.Id] = TimeSpan.Zero;
                    }

                    return await EmbedService.Embed("Now playing", track.Title, track.Url, YouTubeService.GetThumbnail(track.Url), player.Queue.Count, $"{track.Duration:hh\\:mm\\:ss}", $"{TimeSpan.Zero:hh\\:mm\\:ss}", Color.Green);
                }
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return await EmbedService.ErrorEmbed("Something going wrong :no_entry_sign:", exception.Message, Color.Red);
            }
        }

        public async Task<string> PauseAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return ":no_entry_sign: **I'm not connected to a voice channel.**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.PauseAsync();
                    return ":pause_button: **Paused**";
                }

                if (player.PlayerState is PlayerState.Paused)
                {
                    return ":ballot_box_with_check: **Player already paused**";
                }

                return ":no_entry_sign: **No tracks to pause**";
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return ":no_entry_sign: **I'm not connected to a voice channel.**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                    return ":arrow_forward: **Resumed**";
                }

                if (player.PlayerState is PlayerState.Playing)
                {
                    return ":ballot_box_with_check: **Player already resumed**";
                }

                return ":no_entry_sign: **No tracks to resume**";
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<string> SkipAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return ":no_entry_sign: **I'm not connected to a voice channel.**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player is null)
                {
                    return ":no_entry_sign: **Are you sure you are using a bot right now?**";
                }

                LavaTrack track = player.Track;
                if (!_timeLeft.TryGetValue(player.VoiceChannel.Id, out TimeSpan timeLeft))
                {
                    timeLeft = TimeSpan.Zero;
                    _timeLeft.TryAdd(player.VoiceChannel.Id, timeLeft);
                }
                else
                {
                    _timeLeft[player.VoiceChannel.Id] = timeLeft - player.Track.Duration;
                }

                if (_repeatTokens.TryGetValue(player.VoiceChannel.Id, out bool isRepeat) && isRepeat)
                {
                    _repeatTokens[player.VoiceChannel.Id] = false;
                }

                if (player.Queue.Count == 0 && player.PlayerState == PlayerState.Playing)
                {
                    await player.StopAsync();
                    await LogService.InfoAsync("was successfully skipped");
                    return $":ballot_box_with_check: `{track.Title}` **was successfully skipped**";
                }

                if (player.Queue.Count == 0)
                {
                    return ":no_entry_sign: **No tracks to skip**";
                }

                try
                {
                    await player.SkipAsync();
                    await LogService.InfoAsync("was successfully skipped");
                    return $":ballot_box_with_check: `{track.Title}` **was successfully skipped**";
                }
                catch (Exception exception)
                {
                    await LogService.ExceptionAsync(exception);
                    return exception.Message;
                }
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<string> LeaveAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return ":no_entry_sign: **I'm not connected to a voice channel.**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await _lavaNode.LeaveAsync(player.VoiceChannel);
                return ":loudspeaker: **I'm successfully disconnected from the voice channel!**";
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<string> LeftAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return ":no_entry_sign: **I'm not connected to a voice channel.**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player is null)
                {
                    return ":no_entry_sign: **Are you sure you are using a bot right now?**";
                }

                if (player.PlayerState is not PlayerState.Playing)
                {
                    return ":no_entry_sign: **Currently not playing any tracks**";
                }

                LavaTrack track = player.Track;
                TimeSpan time = track.Duration - player.Track.Position;
                await LogService.InfoAsync($"Time left: {time:hh\\:mm\\:ss}");
                return $"**Time left:** `{time:hh\\:mm\\:ss}`";

            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<Embed> QueueAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return await EmbedService.ErrorEmbed("No connection", "I'm not connected to a voice channel", Color.DarkBlue);
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player is null)
                {
                    return await EmbedService.ErrorEmbed("No connection", "Are you sure you are using a bot right now?", Color.DarkBlue);
                }

                if (player.PlayerState is not PlayerState.Playing)
                {
                    return await EmbedService.ErrorEmbed("Something going wrong :no_entry_sign:", "Player stopped",
                        Color.Red);
                }

                StringBuilder stringBuilder = new();
                if (player.Queue.Count < 1 && player.Track != null)
                {
                    return await NowAsync(guild);
                }

                stringBuilder.Append($"**Now Playing**\n[**{player.Track?.Title}**]({player.Track?.Url}) | `Time left: {player.Track?.Duration - player.Track?.Position:hh\\:mm\\:ss}`\n**In queue**\n");
                int trackIndex = 1;
                foreach (LavaTrack track in player.Queue)
                {
                    stringBuilder.Append($"`{trackIndex++}` - [**{track.Title}**]({track.Url}) | `Time left: {track.Duration - track.Position:hh\\:mm\\:ss}`\n");
                }

                return await EmbedService.QueueEmbed("Queue", stringBuilder.ToString(), Color.DarkGrey);

            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return await EmbedService.ErrorEmbed("Something going wrong :no_entry_sign:", exception.Message, Color.Red);
            }
        }

        public async Task<Embed> NowAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return await EmbedService.ErrorEmbed("No connection", "I'm not connected to a voice channel", Color.DarkBlue);
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player is null)
                {
                    return await EmbedService.ErrorEmbed("No connection", "Are you sure you are using a bot right now?", Color.DarkBlue);
                }

                if (player.PlayerState is not PlayerState.Playing)
                {
                    return await EmbedService.ErrorEmbed("No connection", "Currently not playing any tracks",
                        Color.DarkBlue);
                }

                LavaTrack track = player.Track;
                return await EmbedService.NowEmbed("Now playing", track.Title, track.Url, track.Author, track.Duration.ToString(), Color.DarkGrey);

            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return await EmbedService.ErrorEmbed("Something going wrong :no_entry_sign:", exception.Message, Color.Red);
            }
        }

        public async Task<string> SetVolumeAsync(IGuild guild, int volumeValue)
        {
            if (volumeValue is < 0 or > 200)
            {
                return ":no_entry_sign: **Volume value was outside of the bounds.\nThe value must be between 0 and 200!**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volumeValue);
                await LogService.InfoAsync($"Volume is set to {volumeValue}");
                return $":ballot_box_with_check: **Volume is successfully set to {volumeValue}**";
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<string> RepeatAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return "**I'm not connected to a voice channel**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player is null)
                {
                    return "**Are you sure you are using a bot right now?**";
                }

                if (player.PlayerState is not PlayerState.Playing)
                {
                    return ":no_entry_sign: **No tracks to repeat**";
                }

                if (!_repeatTokens.TryGetValue(player.VoiceChannel.Id, out bool isRepeat))
                {
                    isRepeat = true;
                    _repeatTokens.TryAdd(player.VoiceChannel.Id, true);
                }
                else
                {
                    _repeatTokens.TryUpdate(player.VoiceChannel.Id, !isRepeat, isRepeat);
                    isRepeat = _repeatTokens[player.VoiceChannel.Id];
                }

                return isRepeat ? ":ballot_box_with_check: **Repeat was successfully enabled**" : ":ballot_box_with_check: **Repeat was successfully disabled**";

            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<string> ClearAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return "**I'm not connected to a voice channel**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player is null)
                {
                    return "**Are you sure you are using a bot right now?**";
                }

                if (player.PlayerState is not PlayerState.Playing || player.Queue.Count <= 0)
                {
                    return ":no_entry_sign: **Queue is empty**";
                }

                player.Queue.Clear();
                _timeLeft.TryGetValue(player.VoiceChannel.Id, out _);
                _timeLeft[player.VoiceChannel.Id] = player.Track.Duration;
                return ":ballot_box_with_check: **Queue was successfully cleared**";

            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task<string> ShuffleAsync(IGuild guild)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                return "**I'm not connected to a voice channel**";
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);
                if (player is null)
                {
                    return "**Are you sure you are using a bot right now?**";
                }

                if (player.Queue.Count <= 0)
                {
                    return ":no_entry_sign: **Queue is empty**";
                }

                player.Queue.Shuffle();
                return ":ballot_box_with_check: **Queue was successfully shuffled**";

            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
                return exception.Message;
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs trackEnded)
        {
            if (trackEnded.Reason != TrackEndReason.Finished && trackEnded.Reason != TrackEndReason.Stopped)
            {
                return;
            }

            LavaPlayer player = trackEnded.Player;
            if (_repeatTokens.TryGetValue(player.VoiceChannel.Id, out bool isRepeat) && isRepeat)
            {
                LavaTrack currentTrack = trackEnded.Track;
                await player.PlayAsync(currentTrack);
                return;
            }

            if (!player.Queue.TryDequeue(out LavaTrack track))
            {
                _ = InitiateDisconnectAsync(player, TimeSpan.FromMinutes(5));
                return;
            }

            if (track is null)
            {
                return;
            }

            _timeLeft[player.VoiceChannel.Id] -= trackEnded.Track.Duration;
            await player.PlayAsync(track);
        }

        public async Task TrackStarted(TrackStartEventArgs trackStarted)
        {
            if (!_disconnectTokens.TryGetValue(trackStarted.Player.VoiceChannel.Id, out CancellationTokenSource value))
            {
                return;
            }

            if (value.IsCancellationRequested)
            {
                return;
            }

            value.Cancel(true);
            await LogService.InfoAsync("Auto disconnect has been cancelled!");
        }

        public async Task TrackException(TrackExceptionEventArgs arg)
        {
            await LogService.LogAsync($"Track {arg.Track.Title} threw an exception");
            arg.Player.Queue.Enqueue(arg.Track);
        }

        public async Task TrackStuck(TrackStuckEventArgs arg)
        {
            await LogService.LogAsync($"Track {arg.Track.Title} got stuck");
            arg.Player.Queue.Enqueue(arg.Track);
        }

        private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out CancellationTokenSource value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            bool isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
        }
    }
}
