﻿using AlbedoBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AlbedoBot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<CommandService>();
            _services = services;
            SubscribeEvents();
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private void SubscribeEvents()
        {
            _commands.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await LogService.LogAsync(logMessage.Message);
        }

        private Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (socketMessage is null || socketMessage.Author.IsBot)
            {
                return Task.CompletedTask;
            }

            SocketUserMessage userMessage = socketMessage as SocketUserMessage;
            int argPos = 0;
            if (!userMessage.HasStringPrefix(ConfigService.Config.Prefix, ref argPos))
            {
                return Task.CompletedTask;
            }

            SocketCommandContext context = new SocketCommandContext(_client, userMessage);
            Task<IResult> result = _commands.ExecuteAsync(context, argPos, _services);
            return result;
        }
    }
}