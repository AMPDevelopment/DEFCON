using System.Threading.Tasks;
using Defcon.Core.Entities.Enums;
using Defcon.Library.Services.Logs;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Logger
{
    [Group("Log")]
    [Description("Log every event on the guild in a channel of your decision.")]
    [RequireGuild]
    [RequireUserPermissions(Permissions.ManageGuild)]
    public class Log : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly ILogService logService;
        private readonly IRedisDatabase redis;
        private const string description = "Leave it blank to see where the log channel is set to or add enable or disable to do the action.";

        public Log(ILogger logger, IRedisDatabase redis, ILogService logService)
        {
            this.logger = logger;
            this.logService = logService;
            this.redis = redis;
        }

        [Command("JoinedLeft")]
        [Description("Logs as soon as member joined or left the server.")]
        public async Task JoinedLeft(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.JoinedLeft);
        }

        [Command("Invite")]
        [Description("Logs by who the recently member joined.")]
        public async Task Invite(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Invite);
        }

        [Command("Message")]
        [Description("Logs updated and deleted message. Attachments soon:tm:")]
        public async Task Message(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Message);
        }

        [Command("Voice")]
        [Description("Logs every action that has to do with the voice channel.")]
        public async Task Voice(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Voice);
        }

        [Command("Nickname")]
        [Description("Logs every nickname change.")]
        public async Task Nickname(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Nickname);
        }

        [Command("Warn")]
        [Description("Logs the moderation action `warn`.")]
        public async Task Warn(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Warn);
        }

        [Command("Mute")]
        [Description("Logs the moderation action `mute` and `unmute`.")]
        public async Task Mute(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Mute);
        }

        [Command("Kick")]
        [Description("Logs the moderation action `kick`.")]
        public async Task Kick(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Kick);
        }

        [Command("Ban")]
        [Description("Logs the moderation action `ban` and `unban`.")]
        public async Task Ban(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Ban);
        }

        [Command("Server")]
        [Description("Logs server related changes.")]
        public async Task Guild(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.Guild);
        }

        [Command("AutoMod")]
        [Description("Logs the moderation action `auto moderation`.")]
        public async Task AutoMod(CommandContext context, [Description(description)] string status = null)
        {
            await logService.LogInit(context, status, LogType.AutoMod);
        }
    }
}