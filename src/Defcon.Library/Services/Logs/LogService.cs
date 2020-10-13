using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Defcon.Core.Entities.Enums;
using Defcon.Data.Guilds;
using Defcon.Core.Entities.Discord.Embeds;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Library.Services.Logs
{
    public class LogService : ILogService
    {

        private readonly ILogger logger;
        private readonly IRedisDatabase redis;
        private static bool passInformation = true;

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
            var passInformation = true;

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
                        guildData.Logs.Add(new Defcon.Data.Guilds.Log { ChannelId = channel.Id, LogType = logType });

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

        public async Task GuildLogger(BaseDiscordClient client, DiscordGuild guild, object eventArgs, LogType logType)
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

                    switch (eventArgs)
                    {
                        case ChannelCreateEventArgs channelCreateEventArgs:
                            ChannelCreate(client, embed, channelCreateEventArgs);
                            break;
                        case ChannelUpdateEventArgs channelUpdateEventArgs:
                            ChannelUpdate(client, embed, channelUpdateEventArgs);
                            break;
                        case ChannelDeleteEventArgs channelDeleteEventArgs:
                            ChannelDelete(client, guild, logs, channelDeleteEventArgs, guildData, embed);
                            break;
                        case GuildRoleCreateEventArgs guildRoleCreateEventArgs:
                            GuildRoleCreate(client, embed, guildRoleCreateEventArgs);
                            break;
                        case GuildRoleUpdateEventArgs guildRoleUpdateEventArgs:
                            GuildRoleUpdate(client, embed, guildRoleUpdateEventArgs);
                            break;
                        case GuildRoleDeleteEventArgs guildRoleDeleteEventArgs:
                            GuildRoleDelete(client, embed, guildRoleDeleteEventArgs);
                            break;
                        case GuildMemberAddEventArgs memberAddEventArgs:
                            GuildMemberAdd(client, embed, memberAddEventArgs);
                            break;
                        case GuildMemberUpdateEventArgs guildMemberUpdateEventArgs:
                            GuildMemberUpdate(client, embed, guildMemberUpdateEventArgs);
                            break;
                        case GuildMemberRemoveEventArgs guildMemberRemoveEventArgs:
                            GuildMemberRemove(client, embed, guildMemberRemoveEventArgs, logType);
                            break;
                        case GuildBanAddEventArgs guildBanAddEventArgs:
                            GuildBanAdd(client, embed, guildBanAddEventArgs);
                            break;
                        case GuildBanRemoveEventArgs guildBanRemoveEventArgs:
                            // Todo: With or without command
                            break;
                        case InviteCreateEventArgs inviteCreateEventArgs:
                            InviteCreate(client, embed, inviteCreateEventArgs);
                            break;
                        case InviteDeleteEventArgs inviteDeleteEventArgs:
                            InviteDelete(client, embed, inviteDeleteEventArgs);
                            break;
                        case MessageUpdateEventArgs messageUpdateEventArgs:
                            MessageUpdate(client, embed, messageUpdateEventArgs);
                            break;
                        case MessageDeleteEventArgs messageDeleteEventArgs:
                            MessageDelete(client, embed, messageDeleteEventArgs);
                            break;
                        case MessageBulkDeleteEventArgs messageBulkDeleteEventArgs:
                            MessageBulkDelete(client, embed, messageBulkDeleteEventArgs);
                            break;
                        case MessageReactionAddEventArgs messageReactionAddEventArgs:
                            // Why should this be a feature at all?
                            break;
                        case MessageReactionRemoveEventArgs messageReactionRemoveEventArgs:
                            // Why should this be a feature at all?
                            break;
                        case MessageReactionRemoveEmojiEventArgs messageReactionRemoveEmojiEventArgs:
                            // Why should this be a feature at all?
                            break;
                        case MessageReactionsClearEventArgs messageReactionsClearEventArgs:
                            // Why should this be a feature at all?
                            break;
                        case VoiceStateUpdateEventArgs voiceStateUpdateEventArgs:
                        {
                            var before = voiceStateUpdateEventArgs.Before;
                            var after = voiceStateUpdateEventArgs.After;
                            var beforeChannel = voiceStateUpdateEventArgs.Before?.Channel;
                            var afterChannel = voiceStateUpdateEventArgs.After?.Channel;

                            if (beforeChannel != null)
                            {
                                if (afterChannel != null)
                                {
                                    if (beforeChannel != afterChannel)
                                    {
                                        VoiceStateUpdateMemberSwitched(client, embed, voiceStateUpdateEventArgs, beforeChannel, afterChannel);
                                    }
                                    else
                                    {
                                        VoiceStateUpdateMedia(client, embed, voiceStateUpdateEventArgs, before, after);
                                    }

                                    embed.Color = DiscordColor.CornflowerBlue;
                                }
                                else
                                {
                                    VoiceStateUpdateMemberDisconnected(client, embed, voiceStateUpdateEventArgs, beforeChannel);
                                }
                            }
                            else
                            {
                                VoiceStateUpdateMemberConnected(client, embed, voiceStateUpdateEventArgs, afterChannel);
                            }

                            embed.Thumbnail = voiceStateUpdateEventArgs.User.AvatarUrl;
                            embed.Footer = new EmbedFooter()
                            {
                                Text = $"Member: {voiceStateUpdateEventArgs.User.GetUsertag()} | {voiceStateUpdateEventArgs.User.Id}"
                            };
                            break;
                        }
                    }

                    if (passInformation)
                    {
                        await logChannel.SendEmbedMessageAsync(embed);
                    }

                    await Task.CompletedTask.ConfigureAwait(true);
                }
                else
                {
                    guildData.Logs.Remove(log);
                    await redis.ReplaceAsync(RedisKeyNaming.Guild(guild.Id), guildData);
                }
            }
        }

        private static async Task ChannelCreate(BaseDiscordClient client, Embed embed, ChannelCreateEventArgs channelCreateEventArgs)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.New)} {channelCreateEventArgs.Channel.Type.ToString()} channel created";
            var description = new StringBuilder()
                .AppendLine($"{Formatter.InlineCode(channelCreateEventArgs.Channel.Name)} {channelCreateEventArgs.Channel.Mention}");
            
            if (string.IsNullOrWhiteSpace(channelCreateEventArgs.Channel.Topic))
            {
                description.AppendLine($"Topic: {channelCreateEventArgs.Channel.Topic}");
            }
                
            description.AppendLine($"Identity: {Formatter.InlineCode(channelCreateEventArgs.Channel.Id.ToString())}")
                .AppendLine($"Parent: {Formatter.InlineCode(channelCreateEventArgs.Channel.Parent.Name)}")
                .AppendLine($"NSFW: {Formatter.InlineCode(channelCreateEventArgs.Channel.IsNSFW.ToString())}");
            
            if (channelCreateEventArgs.Channel.Type == ChannelType.Voice)
            {
                description.AppendLine($"Bitrate: {Formatter.InlineCode(channelCreateEventArgs.Channel.Bitrate.ToString())}")
                    .AppendLine($"User Limit: {Formatter.InlineCode(channelCreateEventArgs.Channel.UserLimitToString())}");
            }
            
            if (channelCreateEventArgs.Channel.Type == ChannelType.Text)
            {
                description.AppendLine($"Slowmode: {Formatter.InlineCode(channelCreateEventArgs.Channel.PerUserRateLimitToString())}");
            }
            
            embed.Description = description.ToString();
            embed.Color = DiscordColor.SpringGreen;
        }

        private static async Task ChannelUpdate(BaseDiscordClient client, Embed embed, ChannelUpdateEventArgs channelUpdateEventArgs)
        {
            var before = channelUpdateEventArgs.ChannelBefore;
            var after = channelUpdateEventArgs.ChannelAfter;
            
            if (!before.IsPrivate)
            {
                var nameChanged = before.Name != after.Name;
                var topicChanged = before.Topic != after.Topic;
                var parentChanged = before.Parent != after.Parent;
                var isNsfwChanged = before.IsNSFW != after.IsNSFW;
                var bitrateChanged = before.Bitrate != after.Bitrate && after.Type == ChannelType.Voice;
                var userLimitChanged = before.UserLimit != after.UserLimit;
                var perUserRateLimitChanged = before.PerUserRateLimit != after.PerUserRateLimit && after.Type == ChannelType.Text;
                var channelChanged = nameChanged || 
                                     topicChanged || 
                                     parentChanged || 
                                     isNsfwChanged ||
                                     bitrateChanged || 
                                     userLimitChanged || 
                                     perUserRateLimitChanged;
                
                if (channelChanged)
                {
                    embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Update)} {after.Type.ToString()} channel updated";
                    var desciprtion = new StringBuilder()
                        .AppendLine(!nameChanged 
                            ? $"{Formatter.InlineCode(after.Name)} {after.Mention}"
                            : $"{Formatter.InlineCode(before.Name)} to {Formatter.InlineCode(after.Name)} {after.Mention}");
                    
                    if (!string.IsNullOrWhiteSpace(after.Topic))
                    {
                        desciprtion.AppendLine(!topicChanged
                            ? $"Topic: {after.Topic}"
                            : $"Topic: {after.Topic}\nTopic old: {before.Topic}");
                    }
                        
                    desciprtion.AppendLine($"Identity: {Formatter.InlineCode(after.Id.ToString())}")
                        .AppendLine(!parentChanged 
                            ? $"Parent: {Formatter.InlineCode(after.Parent.Name)}"
                            : $"Parent: {Formatter.Underline(before.Parent.Name)} to {Formatter.InlineCode(after.Parent.Name)}")
                        .AppendLine(!isNsfwChanged
                            ? $"NSFW: {Formatter.InlineCode(after.IsNSFW.ToString())}"
                            : $"NSFW: {Formatter.InlineCode(before.IsNSFW.ToString())} to {Formatter.InlineCode(after.IsNSFW.ToString())}");

                    if (after.Type == ChannelType.Voice)
                    {
                        desciprtion.AppendLine(!bitrateChanged
                                ? $"Bitrate: {Formatter.InlineCode(after.Bitrate.ToString())}"
                                : $"Bitrate: {Formatter.InlineCode(before.Bitrate.ToString())} to {Formatter.InlineCode(after.Bitrate.ToString())}")
                            .AppendLine(!userLimitChanged
                                ? $"User Limit: {Formatter.InlineCode(after.UserLimitToString())}"
                                : $"User Limit: {Formatter.InlineCode(before.UserLimitToString())} : {Formatter.InlineCode(after.UserLimitToString())}");
                    }

                    if (after.Type == ChannelType.Text)
                    {
                        desciprtion.AppendLine(!perUserRateLimitChanged
                            ? $"Slowmode: {Formatter.InlineCode(after.PerUserRateLimitToString())}"
                            : $"Slowmode: {Formatter.InlineCode(before.PerUserRateLimitToString())} to {Formatter.InlineCode(after.PerUserRateLimitToString())}");
                    }

                    embed.Description = desciprtion.ToString();
                    embed.Color = DiscordColor.CornflowerBlue;
                }
                else
                {
                    passInformation = false;
                }
            }
        }

        private async Task ChannelDelete(BaseDiscordClient client, DiscordGuild guild, IList<Defcon.Data.Guilds.Log> logs, ChannelDeleteEventArgs channelDeleteEventArgs, Guild guildData, Embed embed)
        {
            if (logs.Any(x => x.ChannelId == channelDeleteEventArgs.Channel.Id))
            {
                var deletedLogChannel = logs.First(x => x.ChannelId == channelDeleteEventArgs.Channel.Id);
                guildData.Logs.Remove(deletedLogChannel);
                await redis.ReplaceAsync(RedisKeyNaming.Guild(guild.Id), guildData);
            }

            if (!channelDeleteEventArgs.Channel.IsPrivate)
            {
                embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Erase)} {channelDeleteEventArgs.Channel.Type.ToString()} channel deleted";
                embed.Description = new StringBuilder().AppendLine($"Name: {Formatter.InlineCode(channelDeleteEventArgs.Channel.Name)}")
                    .AppendLine($"Identity: {Formatter.InlineCode(channelDeleteEventArgs.Channel.Id.ToString())}")
                    .ToString();
                embed.Color = DiscordColor.IndianRed;
            }
        }

        private static async Task GuildRoleCreate(BaseDiscordClient client, Embed embed, GuildRoleCreateEventArgs guildRoleCreateEventArgs)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.New)} Role created";
            embed.Description = new StringBuilder().AppendLine($"Name: {Formatter.InlineCode(guildRoleCreateEventArgs.Role.Name)}")
                .AppendLine($"Identity: {Formatter.InlineCode(guildRoleCreateEventArgs.Role.Id.ToString())}")
                .ToString();
            embed.Color = DiscordColor.SpringGreen;
        }

        private static async Task GuildRoleUpdate(BaseDiscordClient client, Embed embed, GuildRoleUpdateEventArgs guildRoleUpdateEventArgs)
        {
            var before = guildRoleUpdateEventArgs.RoleBefore;
            var after = guildRoleUpdateEventArgs.RoleAfter;

            var nameChanged = before.Name != after.Name;
            var isHoistedChanged = before.IsHoisted != after.IsHoisted;
            var isManagedChanged = before.IsManaged != after.IsManaged;
            var isMentionableChanged = before.IsMentionable != after.IsMentionable;
            var permissionsChanged = before.Permissions != after.Permissions;

            var permissions = permissionsChanged ? await before.Permissions.GetChangedRolesDifference(after.Permissions).ConfigureAwait(true) : string.Empty;

            var roleChanged = nameChanged ||
                              isHoistedChanged ||
                              isManagedChanged ||
                              isMentionableChanged ||
                              permissionsChanged;
            
            if (roleChanged)
            {
                embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Update)} Role updated";
                var description = new StringBuilder().AppendLine(before.Name == after.Name
                        ? $"Name: {Formatter.InlineCode(after.Name)} {after.Mention}"
                        : $"Name: {Formatter.InlineCode(before.Name)} to {Formatter.InlineCode(after.Name)} {after.Mention}")
                    .AppendLine($"Identity: {Formatter.InlineCode(after.Id.ToString())}");

                if (isMentionableChanged)
                {
                    description.AppendLine($"Mentionable: {Formatter.InlineCode(before.IsMentionable.ToString())} to {Formatter.InlineCode(after.IsMentionable.ToString())}");
                }
                
                if (permissionsChanged)
                {
                    embed.Fields = new List<EmbedField> {new EmbedField(){Inline = false, Name = "Changed Permissions", Value = permissions}};
                }
                embed.Description = description.ToString();
                embed.Color = DiscordColor.CornflowerBlue;
            }
            else
            {
                passInformation = false;
            }
        }

        private static async Task GuildRoleDelete(BaseDiscordClient client, Embed embed, GuildRoleDeleteEventArgs guildRoleDeleteEventArgs)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Erase)} Role deleted";
            embed.Description = new StringBuilder().AppendLine($"Name: {Formatter.InlineCode(guildRoleDeleteEventArgs.Role.Name)}")
                .AppendLine($"Identity: {Formatter.InlineCode(guildRoleDeleteEventArgs.Role.Id.ToString())}")
                .ToString();
            embed.Color = DiscordColor.IndianRed;
        }

        private static async Task GuildMemberAdd(BaseDiscordClient client, Embed embed, GuildMemberAddEventArgs memberAddEventArgs)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Joined)} Member joined";
            embed.Description = new StringBuilder().AppendLine($"Username: {Formatter.InlineCode(memberAddEventArgs.Member.GetUsertag())} {memberAddEventArgs.Member.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode(memberAddEventArgs.Member.Id.ToString())}")
                .AppendLine($"Registered: {memberAddEventArgs.Member.CreatedAtLongDateTimeString().Result}")
                .ToString();
            embed.Color = DiscordColor.SpringGreen;
            embed.Thumbnail = memberAddEventArgs.Member.AvatarUrl;
        }

        private static async Task GuildMemberUpdate(BaseDiscordClient client, Embed embed, GuildMemberUpdateEventArgs guildMemberUpdateEventArgs)
        {
            var before = string.IsNullOrWhiteSpace(guildMemberUpdateEventArgs.NicknameBefore) ? guildMemberUpdateEventArgs.Member.Username : guildMemberUpdateEventArgs.NicknameBefore;
            var after = string.IsNullOrWhiteSpace(guildMemberUpdateEventArgs.NicknameAfter) ? guildMemberUpdateEventArgs.Member.Username : guildMemberUpdateEventArgs.NicknameAfter;

            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Edit)} Nickname changed";
            embed.Description = new StringBuilder().AppendLine($"Username: {Formatter.InlineCode(guildMemberUpdateEventArgs.Member.GetUsertag())} {guildMemberUpdateEventArgs.Member.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode($"{guildMemberUpdateEventArgs.Member.Id}")}")
                .ToString();
            embed.Color = DiscordColor.CornflowerBlue;
            embed.Thumbnail = guildMemberUpdateEventArgs.Member.AvatarUrl;
            embed.Fields = new List<EmbedField>
            {
                new EmbedField {Inline = false, Name = "Before", Value = before},
                new EmbedField {Inline = false, Name = "After", Value = after}
            };
            embed.Footer = new EmbedFooter {Text = $"Member Id: {guildMemberUpdateEventArgs.Member.Id}"};
        }

        private async Task GuildMemberRemove(BaseDiscordClient client, Embed embed, GuildMemberRemoveEventArgs guildMemberRemoveEventArgs, LogType logType)
        {
            var roles = guildMemberRemoveEventArgs.Member.Roles.Any()
                ? guildMemberRemoveEventArgs.Member.Roles.Where(x => x.Name != "@everyone")
                    .OrderByDescending(r => r.Position)
                    .Aggregate("", (current, x) => current + $"{x.Mention} ")
                : "None";
            
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Left)} Member left";
            embed.Description = new StringBuilder().AppendLine($"Username: {Formatter.InlineCode(guildMemberRemoveEventArgs.Member.GetUsertag())} {guildMemberRemoveEventArgs.Member.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode(guildMemberRemoveEventArgs.Member.Id.ToString())}").ToString();

            embed.Color = DiscordColor.Gray;
            embed.Thumbnail = guildMemberRemoveEventArgs.Member.AvatarUrl;
            embed.Fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Roles", Value = roles}};
        }

        private async Task GuildBanAdd(BaseDiscordClient client, Embed embed, GuildBanAddEventArgs guildBanAddEventArgs)
        {
            var roles = guildBanAddEventArgs.Member.Roles.Any()
                ? guildBanAddEventArgs.Member.Roles.Where(x => x.Name != "@everyone")
                    .OrderByDescending(r => r.Position)
                    .Aggregate("", (current, x) => current + $"{x.Mention} ")
                : "None";
            
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Left)} Member banned";
            embed.Description = new StringBuilder().AppendLine($"Username: {Formatter.InlineCode(guildBanAddEventArgs.Member.GetUsertag())} {guildBanAddEventArgs.Member.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode(guildBanAddEventArgs.Member.Id.ToString())}").ToString();

            embed.Color = DiscordColor.IndianRed;
            embed.Thumbnail = guildBanAddEventArgs.Member.AvatarUrl;
            embed.Fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Roles", Value = roles}};
        }
        
        private static async Task InviteCreate(BaseDiscordClient client, Embed embed, InviteCreateEventArgs inviteCreateEventArgs)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.New)} Invite created";
            var maxAge = inviteCreateEventArgs.Invite.MaxAge == 0 ? "No limit" : $"{TimeSpan.FromSeconds(inviteCreateEventArgs.Invite.MaxAge).TotalHours}h";
            var maxUses = inviteCreateEventArgs.Invite.MaxUses == 0 ? "Unlimited" : inviteCreateEventArgs.Invite.MaxUses.ToString();
            embed.Description = new StringBuilder().AppendLine($"Code: {Formatter.InlineCode(inviteCreateEventArgs.Invite.Code)}")
                .AppendLine($"{Formatter.InlineCode(inviteCreateEventArgs.Channel.Name)} {inviteCreateEventArgs.Channel.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode(inviteCreateEventArgs.Channel.Id.ToString())}")
                .AppendLine($"Temporary: {Formatter.InlineCode(inviteCreateEventArgs.Invite.IsTemporary.ToString())}")
                .AppendLine($"Max age: {Formatter.InlineCode(maxAge)}")
                .AppendLine($"Max uses: {Formatter.InlineCode(maxUses)}")
                .ToString();
            
            var inviter = inviteCreateEventArgs.Invite.Inviter;
            var inviterDescription = new StringBuilder().AppendLine($"Username: {Formatter.InlineCode(inviter.GetUsertag())} {inviter.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode(inviter.Id.ToString())}")
                .ToString();
            embed.Fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Inviter", Value = inviterDescription}};
            embed.Color = DiscordColor.SpringGreen;
        }
        
        private static async Task InviteDelete(BaseDiscordClient client, Embed embed, InviteDeleteEventArgs inviteDeleteEventArgs)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Erase)} Invite deleted";
            embed.Description = new StringBuilder().AppendLine($"Code: {Formatter.InlineCode(inviteDeleteEventArgs.Invite.Code)}")
                .AppendLine($"{Formatter.InlineCode(inviteDeleteEventArgs.Channel.Name)} {inviteDeleteEventArgs.Channel.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode(inviteDeleteEventArgs.Channel.Id.ToString())}")
                .ToString();
            
            var inviter = inviteDeleteEventArgs.Invite.Inviter;
            var inviterDescription = new StringBuilder().AppendLine($"Username: {Formatter.InlineCode(inviter.GetUsertag())} {inviter.Mention}")
                .AppendLine($"Identity: {Formatter.InlineCode(inviter.Id.ToString())}")
                .ToString();
            embed.Fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Inviter", Value = inviterDescription}};
            embed.Color = DiscordColor.IndianRed;
        }

        private static void MessageUpdate(BaseDiscordClient client, Embed embed, MessageUpdateEventArgs messageUpdateEventArgs)
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

            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Edit)} Message updated";
            embed.Description = new StringBuilder().AppendLine($"Message ({messageUpdateEventArgs.Message.Id}) updated in {messageUpdateEventArgs.Channel.Mention}.")
                .ToString();
            embed.Color = DiscordColor.CornflowerBlue;
            embed.Thumbnail = messageUpdateEventArgs.Author.AvatarUrl;
            embed.Fields = fields;
            embed.Footer = new EmbedFooter {Text = $"Author: {messageUpdateEventArgs.Author.Id} | Message Id: {messageUpdateEventArgs.Message.Id}"};
        }

        private void MessageDelete(BaseDiscordClient client, Embed embed, MessageDeleteEventArgs messageDeleteEventArgs)
        {
            this.logger.Information(!string.IsNullOrWhiteSpace(messageDeleteEventArgs.Message.Content)
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
                embed.Footer = new EmbedFooter {Text = $"Author: {messageDeleteEventArgs.Message.Author.Id} | Message Id: {messageDeleteEventArgs.Message.Id}"};
            }
            else
            {
                fields.First()
                    .Value = "The value is not available due to the message was send while the bot were offline or it's no longer in the cache.";
                embed.Footer = new EmbedFooter {Text = $"Message Id: {messageDeleteEventArgs.Message.Id}"};
            }

            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Erase)} Message deleted";
            embed.Description = new StringBuilder().AppendLine($"Message ({messageDeleteEventArgs.Message.Id}) deleted in {messageDeleteEventArgs.Channel.Mention}.")
                .ToString();
            embed.Color = DiscordColor.IndianRed;
            embed.Thumbnail = thumbnailUrl;
            embed.Fields = fields;
        }

        private static void MessageBulkDelete(BaseDiscordClient client, Embed embed, MessageBulkDeleteEventArgs messageBulkDeleteEventArgs)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Erase)} Message bulk deleted";
            embed.Description = new StringBuilder().AppendLine($"{messageBulkDeleteEventArgs.Messages.Count} messages deleted in {messageBulkDeleteEventArgs.Channel.Mention}.").ToString();
            embed.Color = DiscordColor.IndianRed;
        }

        private void VoiceStateUpdateMemberSwitched(BaseDiscordClient client, Embed embed, VoiceStateUpdateEventArgs voiceStateUpdateEventArgs, DiscordChannel beforeChannel, DiscordChannel afterChannel)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Update)} Member switched voice channel";
            embed.Description = $"Switched from {Formatter.InlineCode(beforeChannel.Name)} to {Formatter.InlineCode(afterChannel.Name)}";
            this.logger.Information($"Member '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) switched the channel from '{beforeChannel.Name}' ({beforeChannel.Id}) to '{afterChannel.Name}' ({afterChannel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
        }

        private void VoiceStateUpdateMedia(BaseDiscordClient client, Embed embed, VoiceStateUpdateEventArgs voiceStateUpdateEventArgs, DiscordVoiceState before, DiscordVoiceState after)
        {
            var actionLog = string.Empty;
            var actionEmbed = string.Empty;
            var stateChanged = false;
            var acceptedEmoji = DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Accepted);
            var deniedEmoji = DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Denied);

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
                var status = before.IsSelfDeafened != after.IsServerDeafened ? "Self Muted and Deafened" : "Self Muted";
                actionLog = $"'{status.ToLower()}' from {before.IsSelfMuted.ToString()} to {after.IsSelfMuted.ToString()}";
                actionEmbed = $"{Formatter.InlineCode(status)} state has been updated from {from} to {to}";
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
            
            // Todo: IsSelfStream and IsSelfVideo (waiting for upcoming lib update)

            if (stateChanged)
            {
                embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Update)} Member state updated";
                embed.Description = actionEmbed;
                this.logger.Information($"Voice state of the user '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) has been updated {actionLog.ToLowerInvariant()} in the channel '{voiceStateUpdateEventArgs.Channel.Name}' ({voiceStateUpdateEventArgs.Channel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
            }
        }

        private void VoiceStateUpdateMemberDisconnected(BaseDiscordClient client, Embed embed, VoiceStateUpdateEventArgs voiceStateUpdateEventArgs, DiscordChannel beforeChannel)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Left)} Member disconnected";
            embed.Description = $"Disconnected {Formatter.InlineCode(beforeChannel.Name)}";
            embed.Color = DiscordColor.IndianRed;
            this.logger.Information($"Member '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) disconnected from the channel '{beforeChannel.Name}' ({beforeChannel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
        }

        private void VoiceStateUpdateMemberConnected(BaseDiscordClient client, Embed embed, VoiceStateUpdateEventArgs voiceStateUpdateEventArgs, DiscordChannel afterChannel)
        {
            embed.Title = $"{DiscordEmoji.FromGuildEmote(client, EmojiLibrary.Joined)} Member connected";
            embed.Description = $"Connected {Formatter.InlineCode(afterChannel.Name)}";
            embed.Color = DiscordColor.SpringGreen;
            this.logger.Information($"Member '{voiceStateUpdateEventArgs.User.GetUsertag()}' ({voiceStateUpdateEventArgs.User.Id}) connected to the channel '{afterChannel.Name}' ({afterChannel.Id}) on the guild '{voiceStateUpdateEventArgs.Guild.Name}' ({voiceStateUpdateEventArgs.Guild.Id}).");
        }

        private static async Task<string> SetLogTypeGetLogLabel(LogType logType)
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
