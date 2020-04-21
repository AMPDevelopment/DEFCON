using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaida.Modules.Logger
{
    [Group("Log")]
    [Description("Log every event on the guild in a channel of your decision.")]
    public class Log : BaseCommandModule
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;

        public Log(ILogger logger, IDatabase redis)
        {
            _logger = logger;
            _redis = redis;
        }

        [Command("JoinedLeft")]
        public async Task JoinedLeft(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.JoinedLeft);
        }

        [Command("Invite")]
        public async Task Invite(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Invite);
        }

        [Command("Message")]
        public async Task Message(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Message);
        }

        [Command("Voice")]
        public async Task Voice(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Voice);
        }

        [Command("Nickname")]
        public async Task Nickname(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Nickname);
        }

        [Command("Warn")]
        public async Task Warn(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Warn);
        }

        [Command("Mute")]
        public async Task Mute(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Mute);
        }

        [Command("Kick")]
        public async Task Kick(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Kick);
        }

        [Command("Ban")]
        public async Task Ban(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Ban);
        }

        [Command("Guild")]
        public async Task Guild(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.Guild);
        }

        private async Task LogSetup(CommandContext context, string status, LogType logType)
        {
            var guild = context.Guild;
            var channel = context.Channel;
            var logLabel = string.Empty;
            switch (logType)
            {
                case LogType.JoinedLeft:
                    logLabel = "JoinedLeftEvent";
                    break;
                case LogType.Invite:
                    logLabel = "InviteEvent";
                    break;
                case LogType.Message:
                    logLabel = "MessageEvent";
                    break;
                case LogType.Voice:
                    logLabel = "VoiceEvent";
                    break;
                case LogType.Nickname:
                    logLabel = "NicknameEvent";
                    break;
                case LogType.Warn:
                    logLabel = "WarnEvent";
                    break;
                case LogType.Mute:
                    logLabel = "MuteEvent";
                    break;
                case LogType.Kick:
                    logLabel = "KickEvent";
                    break;
                case LogType.Ban:
                    logLabel = "BanEvent";
                    break;
                case LogType.Guild:
                    logLabel = "GuildEvent";
                    break;
            }

            if (string.IsNullOrWhiteSpace(status) || status == "enable")
            {
                var key = _redis.StringGet($"{guild.Id}:Logs:{logLabel}");
                if (key.IsNullOrEmpty)
                {
                    _redis.StringSet($"{guild.Id}:Logs:{logLabel}", channel.Id);
                    context.RespondAsync($"The {logLabel} log has been set to this channel.");
                }
                else
                {
                    var loggedChannel = guild.GetChannel((ulong)key);
                    if (loggedChannel.Id == channel.Id)
                    {
                        context.RespondAsync($"The {logLabel} log is already set to this channel.");
                    }
                    else
                    {
                        context.RespondAsync($"The {logLabel} log is already set to {loggedChannel.Mention}.");
                    }
                }
            }
            if (status == "disable")
            {
                _redis.KeyDelete($"{guild.Id}:Logs:{logLabel}");
            }
        }
    }
}
