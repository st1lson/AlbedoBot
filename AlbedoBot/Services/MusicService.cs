using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace AlbedoBot.Services
{
    public sealed class MusicService
    {
        private readonly LavaNode _lavaNode;
        private TimeSpan _timeLeft;

        public MusicService(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        public bool Joined(IGuild guild) => _lavaNode.HasPlayer(guild);

        public async Task<string> JoinAsync(IGuild guild, SocketVoiceChannel voiceChannel, ITextChannel textChannel, int playIndex = 0)
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
                await _lavaNode.JoinAsync(voiceChannel, textChannel);
                return $":ballot_box_with_check: **Joined** `{voiceChannel.Name}`.";
            }
            catch (Exception exception)
            {
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
                var player = _lavaNode.GetPlayer(guild);
                var results = await _lavaNode.SearchYouTubeAsync(trackTitle);

                if (results.LoadStatus.Equals(LoadStatus.LoadFailed) || results.LoadStatus.Equals(LoadStatus.NoMatches))
                {
                    return await EmbedService.ErrorEmbed("No matches found", $"No matches were found for `{trackTitle}`", Color.DarkPurple);
                }

                var track = results.Tracks.FirstOrDefault();
                if (player.PlayerState is PlayerState.Playing)
                {
                    player.Queue.Enqueue(track);
                    var result = await EmbedService.Embed("Added to the queue", track.Title, track.Url, player.Queue.Count + 1, track.Duration.ToString(), _timeLeft.ToString(), Color.Green);
                    _timeLeft += track.Duration;
                    return result;
                }
                else
                {
                    await player.PlayAsync(track);
                    var result = await EmbedService.Embed("Now playing", track.Title, track.Url, player.Queue.Count + 1, track.Duration.ToString(), _timeLeft.ToString(), Color.Green);
                    _timeLeft += track.Duration;
                    return result;
                }
            }
            catch (Exception exception)
            {
                return await EmbedService.ErrorEmbed("Something going wrong :no_entry_sign:", exception.Message, Color.Red);
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
                var player = _lavaNode.GetPlayer(guild);

                if (player is null) return ":no_entry_sign: **Are you sure you are using a bot right now?**";

                var track = player.Track;

                if (player.Queue.Count == 0 && player.PlayerState == PlayerState.Playing)
                {
                    await player.StopAsync();

                    _timeLeft -= track.Duration;

                    return $":ballot_box_with_check: `{track.Title}` **was successfully skipped**";
                }
                else if (player.Queue.Count == 0)
                {
                    return ":no_entry_sign: **No tracks to skip**";
                }

                try
                {
                    await player.SkipAsync();

                    _timeLeft -= track.Duration;

                    return $":ballot_box_with_check: `{track.Title}` **was successfully skipped**";
                }
                catch (Exception exception)
                {
                    return exception.Message;
                }
            }
            catch (Exception exception)
            {
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
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await _lavaNode.LeaveAsync(player.VoiceChannel);

                return $":loudspeaker: **I'm successfully disconnected from the voice channel!";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs trackEnded)
        {
            if (!trackEnded.Reason.ShouldPlayNext())
            {
                return;
            }

            if (!trackEnded.Player.Queue.TryDequeue(out var track))
            {
                return;
            }

            await trackEnded.Player.PlayAsync(track);
        }
    }
}
