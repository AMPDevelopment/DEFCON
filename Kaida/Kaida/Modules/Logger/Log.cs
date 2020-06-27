using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Common.Enums;
using Kaida.Data.Guilds;
using Kaida.Library.Redis;
using Kaida.Library.Services.Logs;
using MoreLinq;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Logger
{
    [Group("Log")]
    [Description("Log every event on the guild in a channel of your decision.")]
    [RequireGuild]
    public class Log : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly ILogService logService;
        private readonly IRedisDatabase redis;

        public Log(ILogger logger, IRedisDatabase redis, ILogService logService)
        {
            this.logger = logger;
            this.logService = logService;
            this.redis = redis;
        }

        [Command("JoinedLeft")]
        public async Task JoinedLeft(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.JoinedLeft);
        }

        [Command("Invite")]
        public async Task Invite(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Invite);
        }

        [Command("Message")]
        public async Task Message(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Message);
        }

        [Command("Voice")]
        public async Task Voice(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Voice);
        }

        [Command("Nickname")]
        public async Task Nickname(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Nickname);
        }

        [Command("Warn")]
        public async Task Warn(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Warn);
        }

        [Command("Mute")]
        public async Task Mute(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Mute);
        }

        [Command("Kick")]
        public async Task Kick(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Kick);
        }

        [Command("Ban")]
        public async Task Ban(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Ban);
        }

        [Command("Guild")]
        public async Task Guild(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.Guild);
        }

        [Command("AutoMod")]
        public async Task AutoMod(CommandContext context, string status = null)
        {
            await logService.LogInit(context, status, LogType.AutoMod);
        }
    }
}