using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kaida.Common.Enums;
using Kaida.Data.Guilds;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Extensions;
using Kaida.Library.Redis;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Log = Kaida.Data.Guilds.Log;

namespace Kaida.Library.Services.Logs
{
    public class LogService : ILogService
    {

        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public LogService(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        public async Task LogInit(CommandContext context, string status, LogType logType)
        {
            var guild = context.Guild;
            var channel = context.Channel;
            var logLabel = await SetLogTypeGetLogLabel(logType);
            var guildData = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guild.Id));
            var log = guildData.Logs.FirstOrDefault(x => x.LogType == logType);

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
                if (status.ToLower().Equals("enable"))
                {
                    if (loggedChannel != null)
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
                    else
                    {
                        guildData.Logs.Add(new Log { ChannelId = channel.Id, LogType = logType });

                        await redis.AddAsync(RedisKeyNaming.Guild(guild.Id), guildData);
                    }

                    respond = await context.RespondAsync($"The {logLabel} log has been set to this channel.");
                }
                else if (status.ToLower().Equals("disable"))
                {
                    if (log != null)
                    {
                        guildData.Logs.Remove(log);

                        await redis.ReplaceAsync(RedisKeyNaming.Guild(guild.Id), guildData);
                        respond = await context.RespondAsync($"The {logLabel} log has been disabled.");
                    }
                    else
                    {
                        respond = await context.RespondAsync("You can't disable something which is not even activated.");
                    }
                }
            }

            if (respond != null)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                respond.DeleteAsync();
            }
        }

        public async Task GuildLogger(DiscordGuild guild, object eventArgs, LogType logType)
        {
            var guildData = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guild.Id));
            var logs = guildData.Logs;
            var log = logs.FirstOrDefault(x => x.LogType == logType);

            if (log != null)
            {
                var logChannel = guild.GetChannel(log.ChannelId);

                if (logChannel != null)
                {
                    var embed = new Embed();

                    if (eventArgs is ChannelCreateEventArgs channelCreateEventArgs)
                    {
                        embed.Title = $"{DiscordEmoji.FromGuildEmote(channelCreateEventArgs.Client, EmojiLibrary.New)} Channel created";
                        embed.Description = new StringBuilder().AppendLine($"Name: `{channelCreateEventArgs.Channel.Name}` {channelCreateEventArgs.Channel.Mention}")
                                                               .AppendLine($"Identity: `{channelCreateEventArgs.Channel.Id}`")
                                                               .AppendLine($"Type: {channelCreateEventArgs.Channel.Type.ToString()}")
                                                               .ToString();
                        embed.Color = DiscordColor.SpringGreen;
                    }
                    else if (eventArgs is ChannelUpdateEventArgs channelUpdateEventArgs)
                    {
                        if (!channelUpdateEventArgs.ChannelBefore.IsPrivate)
                        {
                            embed.Title = $"{DiscordEmoji.FromGuildEmote(channelUpdateEventArgs.Client, EmojiLibrary.Update)} Channel updated";
                            embed.Description = new StringBuilder().AppendLine(channelUpdateEventArgs.ChannelBefore.Name == channelUpdateEventArgs.ChannelAfter.Name
                                                                                   ? $"Name: `{channelUpdateEventArgs.ChannelAfter.Name}` {channelUpdateEventArgs.ChannelAfter.Mention}"
                                                                                   : $"Name: `{channelUpdateEventArgs.ChannelBefore.Name}` to `{channelUpdateEventArgs.ChannelAfter.Name}` {channelUpdateEventArgs.ChannelAfter.Mention}")
                                                                   .AppendLine($"Identity: `{channelUpdateEventArgs.ChannelAfter.Id}`")
                                                                   .AppendLine($"Type: {channelUpdateEventArgs.ChannelAfter.Type.ToString()}")
                                                                   .ToString();
                            embed.Color = DiscordColor.CornflowerBlue;
                        }
                    }
                    else if (eventArgs is ChannelDeleteEventArgs channelDeleteEventArgs)
                    {
                        if (logs.Any(x => x.ChannelId == channelDeleteEventArgs.Channel.Id))
                        {
                            var deletedLogChannel = logs.First(x => x.ChannelId == channelDeleteEventArgs.Channel.Id);
                            guildData.Logs.Remove(deletedLogChannel);
                            await redis.ReplaceAsync(RedisKeyNaming.Guild(guild.Id), guildData);
                        }

                        if (!channelDeleteEventArgs.Channel.IsPrivate)
                        {
                            embed.Title = $"{DiscordEmoji.FromGuildEmote(channelDeleteEventArgs.Client, EmojiLibrary.Erase)} Channel deleted";
                            embed.Description = new StringBuilder().AppendLine($"Name: `{channelDeleteEventArgs.Channel.Name}`")
                                                                   .AppendLine($"Identity: `{channelDeleteEventArgs.Channel.Id}`")
                                                                   .AppendLine($"Type: {channelDeleteEventArgs.Channel.Type.ToString()}")
                                                                   .ToString();
                            embed.Color = DiscordColor.IndianRed;
                        }
                    }
                    else if (eventArgs is GuildRoleCreateEventArgs guildRoleCreateEventArgs)
                    {
                        embed.Title = $"{DiscordEmoji.FromGuildEmote(guildRoleCreateEventArgs.Client, EmojiLibrary.New)} Role created";
                        embed.Description = new StringBuilder().AppendLine($"Name: `{guildRoleCreateEventArgs.Role.Name}`")
                                                               .AppendLine($"Identity: `{guildRoleCreateEventArgs.Role.Id}`")
                                                               .ToString();
                        embed.Color = DiscordColor.SpringGreen;
                    }
                    else if (eventArgs is GuildRoleUpdateEventArgs guildRoleUpdateEventArgs)
                    {
                        embed.Title = $"{DiscordEmoji.FromGuildEmote(guildRoleUpdateEventArgs.Client, EmojiLibrary.Update)} Role updated";
                        embed.Description = new StringBuilder().AppendLine(guildRoleUpdateEventArgs.RoleBefore.Name == guildRoleUpdateEventArgs.RoleAfter.Name
                                                                               ? $"Name: `{guildRoleUpdateEventArgs.RoleAfter.Name}` {guildRoleUpdateEventArgs.RoleAfter.Mention}"
                                                                               : $"Name: `{guildRoleUpdateEventArgs.RoleBefore.Name}` to `{guildRoleUpdateEventArgs.RoleAfter.Name}` {guildRoleUpdateEventArgs.RoleAfter.Mention}")
                                                               .AppendLine($"Identity: `{guildRoleUpdateEventArgs.RoleAfter.Id}`")
                                                               .ToString();
                        embed.Color = DiscordColor.CornflowerBlue;
                    }
                    else if (eventArgs is GuildRoleDeleteEventArgs guildRoleDeleteEventArgs)
                    {
                        embed.Title = $"{DiscordEmoji.FromGuildEmote(guildRoleDeleteEventArgs.Client, EmojiLibrary.Erase)} Role deleted";
                        embed.Description = new StringBuilder().AppendLine($"Name: `{guildRoleDeleteEventArgs.Role.Name}`")
                                                               .AppendLine($"Identity: `{guildRoleDeleteEventArgs.Role.Id}`")
                                                               .ToString();
                        embed.Color = DiscordColor.IndianRed;
                    }
                    else if (eventArgs is GuildMemberAddEventArgs memberAddEventArgs)
                    {
                        embed.Title = $"{DiscordEmoji.FromGuildEmote(memberAddEventArgs.Client, EmojiLibrary.Joined)} Member joined";
                        embed.Description = new StringBuilder().AppendLine($"Username: `{memberAddEventArgs.Member.GetUsertag()}`")
                                                               .AppendLine($"User identity: `{memberAddEventArgs.Member.Id}`")
                                                               .AppendLine($"Registered: {memberAddEventArgs.Member.CreatedAtLongDateTimeString().Result}")
                                                               .ToString();
                        embed.Color = DiscordColor.SpringGreen;
                        embed.ThumbnailUrl = memberAddEventArgs.Member.AvatarUrl;
                    }
                    else if (eventArgs is GuildMemberUpdateEventArgs guildMemberUpdateEventArgs)
                    {
                        var before = string.IsNullOrWhiteSpace(guildMemberUpdateEventArgs.NicknameBefore) ? guildMemberUpdateEventArgs.Member.Username : guildMemberUpdateEventArgs.NicknameBefore;
                        var after = string.IsNullOrWhiteSpace(guildMemberUpdateEventArgs.NicknameAfter) ? guildMemberUpdateEventArgs.Member.Username : guildMemberUpdateEventArgs.NicknameAfter;

                        embed.Title = $"{DiscordEmoji.FromGuildEmote(guildMemberUpdateEventArgs.Client, EmojiLibrary.Edit)} Nickname changed";
                        embed.Description = new StringBuilder().AppendLine($"Mention: {guildMemberUpdateEventArgs.Member.Mention}")
                                                               .AppendLine($"Username: {Formatter.InlineCode(guildMemberUpdateEventArgs.Member.GetUsertag())}")
                                                               .AppendLine($"Identity: {Formatter.InlineCode($"{guildMemberUpdateEventArgs.Member.Id}")}")
                                                               .ToString();
                        embed.Color = DiscordColor.CornflowerBlue;
                        embed.ThumbnailUrl = guildMemberUpdateEventArgs.Member.AvatarUrl;
                        embed.Fields = new List<EmbedField>
                        {
                            new EmbedField {Inline = false, Name = "Before", Value = before},
                            new EmbedField {Inline = false, Name = "After", Value = after}
                        };
                        embed.Footer = new EmbedFooter { Text = $"Member Id: {guildMemberUpdateEventArgs.Member.Id}" };
                    }
                    else if (eventArgs is GuildMemberRemoveEventArgs guildMemberRemoveEventArgs)
                    {
                        var roles = guildMemberRemoveEventArgs.Member.Roles.Any()
                            ? guildMemberRemoveEventArgs.Member.Roles.Where(x => x.Name != "@everyone")
                                                        .OrderByDescending(r => r.Position)
                                                        .Aggregate("", (current, x) => current + $"{x.Mention} ")
                            : "None";

                        embed.Title = $"{DiscordEmoji.FromGuildEmote(guildMemberRemoveEventArgs.Client, EmojiLibrary.Left)} Member left";
                        embed.Description = new StringBuilder().AppendLine($"Username: `{guildMemberRemoveEventArgs.Member.GetUsertag()}`")
                                                               .AppendLine($"User identity: `{guildMemberRemoveEventArgs.Member.Id}`")
                                                               .ToString();
                        embed.Color = DiscordColor.IndianRed;
                        embed.ThumbnailUrl = guildMemberRemoveEventArgs.Member.AvatarUrl;
                        embed.Fields = new List<EmbedField> { new EmbedField { Inline = false, Name = "Roles", Value = roles } };
                    }
                    else if (eventArgs is GuildBanAddEventArgs guildBanAddEventArgs)
                    {

                    }
                    else if (eventArgs is GuildBanRemoveEventArgs guildBanRemoveEventArgs)
                    {

                    }
                    else if (eventArgs is MessageUpdateEventArgs messageUpdateEventArgs)
                    {
                        var fields = new List<EmbedField>
                        {
                            new EmbedField
                            {
                                Inline = false,
                                Name = "Before"
                            }
                        };

                        fields.First()
                              .Value = messageUpdateEventArgs.MessageBefore != null
                            ? messageUpdateEventArgs.MessageBefore.Content
                            : "The value is not available due to the message was send while the bot were offline or it's no longer in the cache.";

                        fields.Add(new EmbedField
                        {
                            Inline = false,
                            Name = "Now",
                            Value = Formatter.MaskedUrl("Jump to message", messageUpdateEventArgs.Message.JumpLink)
                        });

                        embed.Title = $"{DiscordEmoji.FromGuildEmote(messageUpdateEventArgs.Client, EmojiLibrary.Edit)} Message updated";
                        embed.Description = new StringBuilder().AppendLine($"Message ({messageUpdateEventArgs.Message.Id}) updated in {messageUpdateEventArgs.Channel.Mention}.")
                                                               .ToString();
                        embed.Color = DiscordColor.CornflowerBlue;
                        embed.ThumbnailUrl = messageUpdateEventArgs.Author.AvatarUrl;
                        embed.Fields = fields;
                        embed.Footer = new EmbedFooter { Text = $"Author: {messageUpdateEventArgs.Author.Id} | Message Id: {messageUpdateEventArgs.Message.Id}" };
                    }
                    else if (eventArgs is MessageDeleteEventArgs messageDeleteEventArgs)
                    {
                        logger.Information(!string.IsNullOrWhiteSpace(messageDeleteEventArgs.Message.Content)
                                           ? $"The message ({messageDeleteEventArgs.Message.Id}) from '{messageDeleteEventArgs.Message.Author.GetUsertag()}' ({messageDeleteEventArgs.Message.Author.Id}) was deleted in '{messageDeleteEventArgs.Channel.Name}' ({messageDeleteEventArgs.Channel.Id}) on '{messageDeleteEventArgs.Guild.Name}' ({messageDeleteEventArgs.Guild.Id})."
                                           : $"The message ({messageDeleteEventArgs.Message.Id}) was deleted in '{messageDeleteEventArgs.Channel.Name}' ({messageDeleteEventArgs.Channel.Id}) on '{messageDeleteEventArgs.Guild.Name}' ({messageDeleteEventArgs.Guild.Id}).");


                        var thumbnailUrl = string.Empty;
                        var fields = new List<EmbedField>
                        {
                            new EmbedField
                            {
                                Inline = false,
                                Name = "Content"
                            }
                        };

                        if (messageDeleteEventArgs.Message != null)
                        {
                            thumbnailUrl = messageDeleteEventArgs.Message.Author.AvatarUrl;
                            fields.First()
                                  .Value = messageDeleteEventArgs.Message.Content;
                            embed.Footer = new EmbedFooter { Text = $"Author: {messageDeleteEventArgs.Message.Author.Id} | Message Id: {messageDeleteEventArgs.Message.Id}" };
                        }
                        else
                        {
                            fields.First()
                                  .Value = "The value is not available due to the message was send while the bot were offline or it's no longer in the cache.";
                            embed.Footer = new EmbedFooter { Text = $"Message Id: {messageDeleteEventArgs.Message.Id}" };
                        }

                        embed.Title = $"{DiscordEmoji.FromGuildEmote(messageDeleteEventArgs.Client, EmojiLibrary.Erase)} Message deleted";
                        embed.Description = new StringBuilder().AppendLine($"Message ({messageDeleteEventArgs.Message.Id}) deleted in {messageDeleteEventArgs.Channel.Mention}.")
                                                               .ToString();
                        embed.Color = DiscordColor.IndianRed;
                        embed.ThumbnailUrl = thumbnailUrl;
                        embed.Fields = fields;
                    }
                    else if (eventArgs is MessageBulkDeleteEventArgs messageBulkDeleteEventArgs)
                    {
                        embed.Title = $"{DiscordEmoji.FromGuildEmote(messageBulkDeleteEventArgs.Client, EmojiLibrary.Erase)} Message bulk deleted";
                        embed.Description = new StringBuilder().AppendLine($"{messageBulkDeleteEventArgs.Messages.Count} messages deleted in {messageBulkDeleteEventArgs.Channel.Mention}.")
                                                               .ToString();
                        embed.Color = DiscordColor.IndianRed;
                    }
                    else if (eventArgs is MessageReactionAddEventArgs messageReactionAddEventArgs)
                    {
                        // Why should this be a feature at all?
                    }
                    else if (eventArgs is MessageReactionRemoveEventArgs messageReactionRemoveEventArgs)
                    {
                        // Why should this be a feature at all?
                    }
                    else if (eventArgs is MessageReactionRemoveEmojiEventArgs messageReactionRemoveEmojiEventArgs)
                    {
                        // Why should this be a feature at all?
                    }
                    else if (eventArgs is MessageReactionsClearEventArgs messageReactionsClearEventArgs)
                    {
                        // Why should this be a feature at all?
                    }
                    else if (eventArgs is VoiceStateUpdateEventArgs voiceStateUpdateEventArgs)
                    {
                        var channel = voiceStateUpdateEventArgs.Channel;
                        var before = voiceStateUpdateEventArgs.Before;
                        var after = voiceStateUpdateEventArgs.After;
                        var beforeChannel = voiceStateUpdateEventArgs.Before?.Channel;
                        var afterChannel = voiceStateUpdateEventArgs.After?.Channel;
                        var acceptedEmoji = DiscordEmoji.FromGuildEmote(voiceStateUpdateEventArgs.Client, EmojiLibrary.Accepted);
                        var deniedEmoji = DiscordEmoji.FromGuildEmote(voiceStateUpdateEventArgs.Client, EmojiLibrary.Denied);

                        if (beforeChannel != null)
                        {
                            if (afterChannel != null)
                            {
                                if (beforeChannel != afterChannel)
                                {
                                    embed.Title = $"{DiscordEmoji.FromGuildEmote(voiceStateUpdateEventArgs.Client, EmojiLibrary.Update)} Member switched voice channel";
                                    embed.Description = $"Switched from {Formatter.InlineCode(beforeChannel.Name)} to {Formatter.InlineCode(afterChannel.Name)}";
                                    logger.Information($"Member '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) switched the channel from '{beforeChannel.Name}' ({beforeChannel.Id}) to '{afterChannel.Name}' ({afterChannel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
                                }
                                else
                                {
                                    var actionLog = string.Empty;
                                    var actionEmbed = string.Empty;
                                    var stateChanged = false;

                                    if (before.IsSelfDeafened != after.IsSelfDeafened)
                                    {
                                        stateChanged = true;
                                        var from = before.IsSelfDeafened ? acceptedEmoji : deniedEmoji;
                                        var to = after.IsSelfDeafened ? acceptedEmoji : deniedEmoji;
                                        actionLog = $"'self deafened' from {before.IsSelfDeafened.ToString()} to {after.IsSelfDeafened.ToString()}";
                                        actionEmbed = $"{Formatter.InlineCode("Self Deafened")} state has been updated from {from} to {to}";
                                    }

                                    if (before.IsSelfMuted != after.IsSelfMuted)
                                    {
                                        stateChanged = true;
                                        var from = before.IsSelfMuted ? acceptedEmoji : deniedEmoji;
                                        var to = after.IsSelfMuted ? acceptedEmoji : deniedEmoji;
                                        actionLog = $"'self muted' from {before.IsSelfMuted.ToString()} to {after.IsSelfMuted.ToString()}";
                                        actionEmbed = $"{Formatter.InlineCode("Self Muted")} state has been updated from {from} to {to}";
                                    }

                                    if (before.IsServerDeafened != after.IsServerDeafened)
                                    {
                                        stateChanged = true;
                                        var from = before.IsServerDeafened ? acceptedEmoji : deniedEmoji;
                                        var to = after.IsServerDeafened ? acceptedEmoji : deniedEmoji;
                                        actionLog = $"'server deafened' from {before.IsServerDeafened.ToString()} to {after.IsServerDeafened.ToString()}";
                                        actionEmbed = $"{Formatter.InlineCode("Server Deafened")} state has been updated from {from} to {to}";
                                    }

                                    if (before.IsServerMuted != after.IsServerMuted)
                                    {
                                        stateChanged = true;
                                        var from = before.IsServerMuted ? acceptedEmoji : deniedEmoji;
                                        var to = after.IsServerMuted ? acceptedEmoji : deniedEmoji;
                                        actionLog = $"'server muted' from {before.IsServerMuted.ToString()} to {after.IsServerMuted.ToString()}";
                                        actionEmbed = $"{Formatter.InlineCode("Server Muted")} state has been updated from {from} to {to}";
                                    }

                                    if (before.IsSuppressed != after.IsSuppressed)
                                    {
                                        stateChanged = true;
                                        var from = before.IsSuppressed ? acceptedEmoji : deniedEmoji;
                                        var to = after.IsSuppressed ? acceptedEmoji : deniedEmoji;
                                        actionLog = $"'suppressed' from {before.IsSuppressed.ToString()} to {after.IsSuppressed.ToString()}";
                                        actionEmbed = $"{Formatter.InlineCode("Suppressed")} state has been updated from {from} to {to}";
                                    }

                                    if (stateChanged)
                                    {
                                        embed.Title = $"{DiscordEmoji.FromGuildEmote(voiceStateUpdateEventArgs.Client, EmojiLibrary.Update)} Member state updated";
                                        embed.Description = actionEmbed;
                                        logger.Information($"Voice state of the user '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) has been updated {actionLog.ToLowerInvariant()} in the channel '{voiceStateUpdateEventArgs.Channel.Name}' ({voiceStateUpdateEventArgs.Channel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
                                    }
                                }

                                embed.Color = DiscordColor.CornflowerBlue;
                            }
                            else
                            {
                                embed.Title = $"{DiscordEmoji.FromGuildEmote(voiceStateUpdateEventArgs.Client, EmojiLibrary.Left)} Member disconnected";
                                embed.Description = $"Disconnected {Formatter.InlineCode(beforeChannel.Name)}";
                                embed.Color = DiscordColor.IndianRed;
                                logger.Information($"Member '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) disconnected from the channel '{beforeChannel.Name}' ({beforeChannel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
                            }
                        }
                        else
                        {
                            embed.Title = $"{DiscordEmoji.FromGuildEmote(voiceStateUpdateEventArgs.Client, EmojiLibrary.Joined)} Member connected";
                            embed.Description = $"Connected {Formatter.InlineCode(afterChannel.Name)}";
                            embed.Color = DiscordColor.SpringGreen; ;
                            logger.Information($"Member '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) connected to the channel '{afterChannel.Name}' ({afterChannel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
                        }

                        embed.ThumbnailUrl = voiceStateUpdateEventArgs.User.AvatarUrl;
                        embed.Footer = new EmbedFooter()
                        {
                            Text = $"Member: {voiceStateUpdateEventArgs.User.GetUsertag()} | {voiceStateUpdateEventArgs.User.Id}"
                        };
                    }

                    await logChannel.SendEmbedMessageAsync(embed);
                }
                else
                {
                    guildData.Logs.Remove(log);
                    await redis.ReplaceAsync(RedisKeyNaming.Guild(guild.Id), guildData);
                }
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
