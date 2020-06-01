using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Common.Enums;
using Kaida.Data.Guilds;
using Kaida.Library.Redis;
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
        private readonly IRedisDatabase redis;

        public Log(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
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

        [Command("AutoMod")]
        public async Task AutoMod(CommandContext context, string status = null)
        {
            await LogSetup(context, status, LogType.AutoMod);
        }

        private async Task LogSetup(CommandContext context, string status, LogType logType)
        {
            var guild = context.Guild;
            var channel = context.Channel;
            var logLabel = await SetLogTypeGetLogLabel(logType);
            var guildData = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guild.Id));
            var log = guildData.Logs.First(x => x.LogType == logType);

            DiscordChannel loggedChannel = null;
            DiscordMessage respond = null;

            if (log != null)
            {
                loggedChannel = guild.GetChannel(log.ChannelId);
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                if (loggedChannel != null)
                {
                    respond = await context.RespondAsync($"The {logLabel} log has been set to {loggedChannel.Mention}");
                }
                else
                {
                    respond = await context.RespondAsync($"The {logLabel} log has not been set to any channel.");
                }
            }
            else
            {
                if (log == null)
                {
                    guildData.Logs.Add(new Data.Guilds.Log()
                    {
                        ChannelId = channel.Id,
                        LogType = logType
                    });

                    redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(guild.Id), guildData);
                    respond = await context.RespondAsync($"The {logLabel} log has been set to this channel.");
                }
                else
                {
                    if (loggedChannel.Id == channel.Id)
                    {
                        respond = await context.RespondAsync($"The {logLabel} log is already set to this channel.");
                    }
                    else
                    {
                        respond = await context.RespondAsync($"The {logLabel} log is already set to {loggedChannel.Mention}.");
                    }
                }
            }

            if (status == "disable")
            {
                if (log != null)
                {
                    guildData.Logs.Remove(log);

                    await redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(guild.Id), guildData);
                    respond = await context.RespondAsync($"The {logLabel} log has been disabled.");
                }
                else
                {
                    respond = await context.RespondAsync($"You can't disable something which is not even activated.");
                }
            }

            if (respond != null)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                respond.DeleteAsync();
            }
        }

        private async Task<string> SetLogTypeGetLogLabel(LogType logType)
        {
            var logLabel = string.Empty;

            switch (logType)
            {
                case LogType.JoinedLeft:
                    logLabel = "JoinedLeft";

                    break;
                case LogType.Invite:
                    logLabel = "Invite";

                    break;
                case LogType.Message:
                    logLabel = "Message";

                    break;
                case LogType.Voice:
                    logLabel = "Voice";

                    break;
                case LogType.Nickname:
                    logLabel = "Nickname";

                    break;
                case LogType.Warn:
                    logLabel = "Warn";

                    break;
                case LogType.Mute:
                    logLabel = "Mute";

                    break;
                case LogType.Kick:
                    logLabel = "Kick";

                    break;
                case LogType.Ban:
                    logLabel = "Ban";

                    break;
                case LogType.Guild:
                    logLabel = "Guild";

                    break;
                case LogType.AutoMod:
                    logLabel = "AutoMod";

                    break;
            }

            return logLabel;
        }
    }
}