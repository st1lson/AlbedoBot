using System;
using System.Threading.Tasks;
using AlbedoBot.Handlers;
using AlbedoBot.Services;
using Victoria;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AlbedoBot.Core
{
    public sealed class AlbedoBotClient
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly MusicService _musicService;
        private readonly ConfigService _configService;

        public AlbedoBotClient()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _configService = _services.GetRequiredService<ConfigService>();
            _musicService = _services.GetRequiredService<MusicService>();

            SubscribeEvents();
        }

        public async Task InitializeAsync()
        {
            await _configService.InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, ConfigService.Config.Token);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();

            await Task.Delay(-1);
        }

        private void SubscribeEvents()
        {
            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackStarted += _musicService.TrackStarted;
            _lavaNode.OnTrackEnded += _musicService.TrackEnded;
        }

        private async Task ReadyAsync()
        {
            try
            {
                await _lavaNode.ConnectAsync();
                await _client.SetGameAsync(ConfigService.Config.GameStatus);
            }
            catch (Exception exception)
            {
                await LogService.ExceptionAsync(exception);
            }
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await LogService.LogAsync(logMessage);
        }


        private ServiceProvider ConfigureServices() =>
            new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LavaNode>()
                .AddSingleton(new LavaConfig())
                .AddSingleton<MusicService>()
                .AddSingleton<ConfigService>()
                .BuildServiceProvider();
    }
}