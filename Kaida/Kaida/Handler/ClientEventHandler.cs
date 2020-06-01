using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Kaida.Common.Enums;
using Kaida.Data.Guilds;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Extensions;
using Kaida.Library.Redis;
using Kaida.Library.Services.Reactions;
using MoreLinq.Extensions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Handler
{
    public class ClientEventHandler
    {
        private readonly DiscordShardedClient client;
        private readonly ILogger logger;
        private readonly IReactionService reactionService;
        private readonly IRedisDatabase redis;
        private int activityIndex;
        private Timer timer;

        public ClientEventHandler(DiscordShardedClient client, ILogger logger, IRedisDatabase redis, IReactionService reactionService)
        {
            this.client = client;
            this.logger = logger;
            this.redis = redis;
            this.reactionService = reactionService;

            this.logger.Information("Register events...");
            this.client.Ready += Ready;
            this.client.GuildDownloadCompleted += GuildDownloadCompleted;
            this.client.GuildAvailable += GuildAvailable;
            this.client.GuildUnavailable += GuildUnavailable;
            this.client.GuildCreated += GuildCreated;
            this.client.GuildUpdated += GuildUpdated;
            this.client.GuildDeleted += GuildDeleted;
            this.client.GuildEmojisUpdated += GuildEmojisUpdated;
            this.client.GuildIntegrationsUpdated += GuildIntegrationsUpdated;
            this.client.ChannelCreated += ChannelCreated;
            this.client.ChannelDeleted += ChannelDeleted;
            this.client.ChannelUpdated += ChannelUpdated;
            this.client.GuildRoleCreated += GuildRoleCreated;
            this.client.GuildRoleUpdated += GuildRoleUpdated;
            this.client.GuildRoleDeleted += GuildRoleDeleted;
            this.client.GuildMemberAdded += GuildMemberAdded;
            this.client.GuildMemberRemoved += GuildMemberRemoved;
            this.client.GuildMemberUpdated += GuildMemberUpdated;
            this.client.GuildBanAdded += GuildBanAdded;
            this.client.GuildBanRemoved += GuildBanRemoved;
            this.client.DmChannelCreated += DmChannelCreated;
            this.client.DmChannelDeleted += DmChannelDeleted;
            this.client.MessageCreated += MessageCreated;
            this.client.MessageUpdated += MessageUpdated;
            this.client.MessageDeleted += MessageDeleted;
            this.client.MessagesBulkDeleted += MessagesBulkDeleted;
            this.client.MessageReactionAdded += MessageReactionAdded;
            this.client.MessageReactionRemoved += MessageReactionRemoved;
            this.client.MessageReactionRemovedEmoji += MessageReactionRemovedEmoji;
            this.client.MessageReactionsCleared += MessageReactionsCleared;
            this.client.VoiceServerUpdated += VoiceServerUpdated;
            this.client.VoiceStateUpdated += VoiceStateUpdated;
            this.client.SocketOpened += SocketOpened;
            this.client.SocketClosed += SocketClosed;
            this.client.SocketErrored += SocketErrored;
            this.client.ClientErrored += ClientErrored;
            this.client.UnknownEvent += UnknownEvent;
            this.logger.Information("Registered all events!");
        }

        private async Task Ready(ReadyEventArgs e)
        {
            logger.Information("Client is ready!");

            timer = new Timer(async _ =>
            {
                var guilds = e.Client.Guilds.Values.ToList();
                var guildsCount = guilds.Count;
                var totalUsers = new List<DiscordUser>();

                foreach (var guild in guilds)
                {
                    var members = await guild.GetAllMembersAsync();
                    totalUsers.AddRange(members.Where(x => x.IsBot == false));
                }

                var uniqueUsers = totalUsers.DistinctBy(x => x.Id)
                                            .ToList()
                                            .Count;

                var activities = new List<DiscordActivity>
                {
                    new DiscordActivity {ActivityType = ActivityType.Watching, Name = $"{guildsCount} servers"}, 
                    new DiscordActivity {ActivityType = ActivityType.ListeningTo, Name = $"{uniqueUsers} unique users"}
                };
                await client.UpdateStatusAsync(activities.ElementAtOrDefault(activityIndex), UserStatus.Online, DateTimeOffset.UtcNow);
                activityIndex = activityIndex + 1 == activities.Count ? 0 : activityIndex + 1;
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
        }

        private Task GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
        {
            logger.Information("Client downloaded all guilds successfully.");

            return Task.CompletedTask;
        }

        private Task GuildAvailable(GuildCreateEventArgs e)
        {
            redis.InitGuild(e.Guild.Id);
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became available.");

            return Task.CompletedTask;
        }

        private Task GuildUnavailable(GuildDeleteEventArgs e)
        {
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became unavailable.");

            return Task.CompletedTask;
        }

        private Task GuildCreated(GuildCreateEventArgs e)
        {
            logger.Information($"Joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            redis.InitGuild(e.Guild.Id);

            return Task.CompletedTask;
        }

        private Task GuildUpdated(GuildUpdateEventArgs e)
        {
            logger.Information($"Guild '{e.GuildAfter.Name}' ({e.GuildAfter.Id}) has been updated.");

            return Task.CompletedTask;
        }

        private Task GuildDeleted(GuildDeleteEventArgs e)
        {
            logger.Information($"Left the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildEmojisUpdated(GuildEmojisUpdateEventArgs e)
        {
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) has updated their emojis.");

            return Task.CompletedTask;
        }

        private Task GuildIntegrationsUpdated(GuildIntegrationsUpdateEventArgs e)
        {
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) has updated their integrations.");

            return Task.CompletedTask;
        }

        private Task ChannelCreated(ChannelCreateEventArgs e)
        {
            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Guild);

            if (log != null)
            {
                var logChannel = e.Guild.GetChannel(log.ChannelId);

                var embed = new Embed
                {
                    Title = ":KaidaNew: Channel created",
                    Description = new StringBuilder().AppendLine($"Name: `{e.Channel.Name}` {e.Channel.Mention}")
                                                     .AppendLine($"Identity: `{e.Channel.Id}`")
                                                     .AppendLine($"Type: {e.Channel.Type.ToString()}")
                                                     .ToString(),
                    Color = DiscordColor.SpringGreen
                };

                logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
            }

            logger.Information($"Channel '{e.Channel.Name}' ({e.Channel.Id}) has been created on guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task ChannelUpdated(ChannelUpdateEventArgs e)
        {
            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Guild);

            if (!e.ChannelBefore.IsPrivate)
            {
                if (log != null)
                {
                    var logChannel = e.Guild.GetChannel(log.ChannelId);

                    var description = new StringBuilder();

                    description.AppendLine(e.ChannelBefore.Name == e.ChannelAfter.Name ? $"Name: `{e.ChannelAfter.Name}` {e.ChannelAfter.Mention}" : $"Name: `{e.ChannelBefore.Name}` to `{e.ChannelAfter.Name}` {e.ChannelAfter.Mention}");

                    description.AppendLine($"Identity: `{e.ChannelAfter.Id}`")
                               .AppendLine($"Type: {e.ChannelAfter.Type.ToString()}");

                    var embed = new Embed
                    {
                        Title = ":KaidaUpdate: Channel updated", 
                        Description = description.ToString(), 
                        Color = DiscordColor.SpringGreen
                    };

                    logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
                }

                logger.Information($"Channel '{e.ChannelAfter.Name}' ({e.ChannelAfter.Id}) has been updated on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            }

            return Task.CompletedTask;
        }

        private Task ChannelDeleted(ChannelDeleteEventArgs e)
        {
            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Guild);

            if (!e.Channel.IsPrivate)
            {
                if (log != null)
                {
                    var logChannel = e.Guild.GetChannel(log.ChannelId);

                    var description = new StringBuilder();

                    description.AppendLine($"Identity: `{e.Channel.Id}`")
                               .AppendLine($"Type: {e.Channel.Type.ToString()}");

                    var embed = new Embed
                    {
                        Title = ":KaidaErase: Channel updated", 
                        Description = description.ToString(), 
                        Color = DiscordColor.SpringGreen
                    };

                    logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
                }

                logger.Information($"Channel '{e.Channel.Name}' ({e.Channel.Id}) has been deleted on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            }

            return Task.CompletedTask;
        }

        private Task GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Guild);

            if (log != null)
            {
                var logChannel = e.Guild.GetChannel(log.ChannelId);

                var embed = new Embed
                {
                    Title = ":KaidaNew: Role created",
                    Description = new StringBuilder().AppendLine($"Name: `{e.Role.Name}`")
                                                     .AppendLine($"Identity: `{e.Role.Id}`")
                                                     .ToString(),
                    Color = DiscordColor.SpringGreen
                };

                logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
            }

            logger.Information($"Role '{e.Role.Name}' ({e.Role.Id}) has been created on guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Guild);

            if (log != null)
            {
                var logChannel = e.Guild.GetChannel(log.ChannelId);

                var description = new StringBuilder();

                description.AppendLine(e.RoleBefore.Name == e.RoleAfter.Name ? $"Name: `{e.RoleAfter.Name}` {e.RoleAfter.Mention}" : $"Name: `{e.RoleBefore.Name}` to `{e.RoleAfter.Name}` {e.RoleAfter.Mention}");

                description.AppendLine($"Identity: `{e.RoleAfter.Id}`");

                var embed = new Embed
                {
                    Title = ":KaidaUpdate: Role updated", 
                    Description = description.ToString(), 
                    Color = DiscordColor.SpringGreen
                };

                logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
            }

            logger.Information($"Role '{e.RoleAfter.Name}' ({e.RoleAfter.Id}) has been updated on guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Guild);

            if (log != null)
            {
                var logChannel = e.Guild.GetChannel(log.ChannelId);

                var embed = new Embed
                {
                    Title = ":KaidaErase: Role deleted",
                    Description = new StringBuilder().AppendLine($"Name: `{e.Role.Name}`")
                                                     .AppendLine($"Identity: `{e.Role.Id}`")
                                                     .ToString(),
                    Color = DiscordColor.SpringGreen
                };

                logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
            }

            logger.Information($"Role '{e.Role.Name}' ({e.Role.Id}) has been deleted on guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            redis.InitUser(e.Member.Id);

            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.JoinedLeft);

            if (log != null)
            {
                var logChannel = e.Guild.GetChannel(log.ChannelId);

                var embed = new Embed
                {
                    Title = ":KaidaJoined: Member joined",
                    Description = new StringBuilder().AppendLine($"Username: `{e.Member.GetUsertag()}`")
                                                     .AppendLine($"User identity: `{e.Member.Id}`")
                                                     .AppendLine($"Registered: {e.Member.CreatedAtLongDateTimeString().Result}")
                                                     .ToString(),
                    Color = DiscordColor.SpringGreen,
                    ThumbnailUrl = e.Member.AvatarUrl
                };

                logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
            }

            logger.Information($"Member '{e.Member.GetUsertag()}' ({e.Member.Id}) has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.JoinedLeft);

            if (log != null)
            {
                var logChannel = e.Guild.GetChannel(log.ChannelId);

                var roles = e.Member.Roles.Any()
                    ? e.Member.Roles.Where(x => x.Name != "@everyone")
                       .OrderByDescending(r => r.Position)
                       .Aggregate("", (current, x) => current + $"{x.Mention} ")
                    : "None";

                var fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Roles", Value = roles}};

                var embed = new Embed
                {
                    Title = ":KaidaLeft: Member left",
                    Description = new StringBuilder().AppendLine($"Username: `{e.Member.GetUsertag()}`")
                                                     .AppendLine($"User identity: `{e.Member.Id}`")
                                                     .ToString(),
                    Color = DiscordColor.IndianRed,
                    ThumbnailUrl = e.Member.AvatarUrl,
                    Fields = fields
                };

                logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
            }

            logger.Information($"Member '{e.Member.GetUsertag()}' ({e.Member.Id}) has left the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            var before = string.IsNullOrWhiteSpace(e.NicknameBefore) ? e.Member.Username : e.NicknameBefore;
            var after = string.IsNullOrWhiteSpace(e.NicknameAfter) ? e.Member.Username : e.NicknameAfter;

            if (before == after) return Task.CompletedTask;

            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Nickname);

            if (log != null)
            {
                var channel = e.Guild.GetChannel(log.ChannelId);

                var fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Before", Value = before}, new EmbedField {Inline = false, Name = "After", Value = after}};

                var embed = new Embed
                {
                    Title = ":KaidaEdit: Nickname changed",
                    Description = new StringBuilder().AppendLine($"Mention: {e.Member.Mention}")
                                                     .AppendLine($"Username: {Formatter.InlineCode(e.Member.GetUsertag())}")
                                                     .AppendLine($"Identity: {Formatter.InlineCode($"{e.Member.Id}")}")
                                                     .ToString(),
                    Color = DiscordColor.CornflowerBlue,
                    ThumbnailUrl = e.Member.AvatarUrl,
                    Fields = fields,
                    Footer = new EmbedFooter {Text = $"Member Id: {e.Member.Id}"}
                };

                channel.SendEmbedMessageAsync(embed);
            }

            logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has changed there nickname from '{before}' to '{after}' on '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildBanAdded(GuildBanAddEventArgs e)
        {
            logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been banned from '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been unbanned from '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task DmChannelCreated(DmChannelCreateEventArgs e)
        {
            var user = e.Channel.Recipients.First(x => !x.IsBot);
            logger.Information($"Direct message with '{user.GetUsertag()}' ({user.Id}) has been created.");

            return Task.CompletedTask;
        }

        private Task DmChannelDeleted(DmChannelDeleteEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task MessageCreated(MessageCreateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task MessageUpdated(MessageUpdateEventArgs e)
        {
            if (e.Author.IsBot) return Task.CompletedTask;

            if (e.Channel.IsPrivate)
            {
                logger.Information($"The message ({e.Message.Id}) from '{e.Author.GetUsertag()}' ({e.Author.Id}) was updated in the direct message.");

                return Task.CompletedTask;
            }

            var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                           .GetAwaiter()
                           .GetResult()
                           .Logs.FirstOrDefault(x => x.LogType == LogType.Message);

            if (log != null)
            {
                var logChannel = e.Guild.GetChannel(log.ChannelId);
                var contentNow = e.Message.Content;
                var fields = new List<EmbedField>();

                if (e.MessageBefore != null)
                {
                    var contentBefore = e.MessageBefore.Content;

                    if (contentBefore.Equals(contentNow)) return Task.CompletedTask;

                    fields.Add(new EmbedField
                    {
                        Inline = false, 
                        Name = "Before", 
                        Value = contentBefore
                    });
                }
                else
                {
                    fields.Add(new EmbedField
                    {
                        Inline = false, 
                        Name = "Before", 
                        Value = "The value is not available due to the message was send while the bot were offline or it's no longer in the cache."
                    });
                }

                fields.Add(new EmbedField
                {
                    Inline = false, 
                    Name = "After", 
                    Value = contentNow
                });

                var embed = new Embed
                {
                    Title = ":KaidaEdit: Message updated",
                    Description = new StringBuilder().AppendLine($"Message ({e.Message.Id}) updated in {e.Channel.Mention}.")
                                                     .AppendLine($"[Jump to message]({e.Message.JumpLink})")
                                                     .ToString(),
                    Color = DiscordColor.Orange,
                    ThumbnailUrl = e.Author.AvatarUrl,
                    Fields = fields,
                    Footer = new EmbedFooter {Text = $"Author: {e.Author.Id} | Message Id: {e.Message.Id}"}
                };

                logChannel.SendEmbedMessageAsync(embed);
            }

            logger.Information($"The message ({e.Message.Id}) from '{e.Author.GetUsertag()}' ({e.Author.Id}) was updated in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task MessageDeleted(MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
            {
                logger.Information(!string.IsNullOrWhiteSpace(e.Message.Content) 
                                       ? $"The message ({e.Message.Id}) from '{e.Message.Author.GetUsertag()}' ({e.Message.Author.Id}) was deleted in the direct message." 
                                       : $"The message ({e.Message.Id}) was deleted in the direct message.");

                return Task.CompletedTask;
            }

            if (e.Message.Author.IsBot) return Task.CompletedTask;

            var guild = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                             .GetAwaiter()
                             .GetResult();

            if (e.Message.Content.StartsWith(guild.Prefix)) return Task.CompletedTask;

            logger.Information(!string.IsNullOrWhiteSpace(e.Message.Content)
                                   ? $"The message ({e.Message.Id}) from '{e.Message.Author.GetUsertag()}' ({e.Message.Author.Id}) was deleted in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id})."
                                   : $"The message ({e.Message.Id}) was deleted in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");

            var log = guild.Logs.FirstOrDefault(x => x.LogType == LogType.Message);

            if (log != null)
            {
                var thumbnailUrl = string.Empty;
                var fields = new List<EmbedField>();
                EmbedFooter footer;

                var logChannel = e.Guild.GetChannel(log.ChannelId);

                if (!string.IsNullOrWhiteSpace(e.Message.Content))
                {
                    thumbnailUrl = e.Message.Author.AvatarUrl;
                    fields.Add(new EmbedField
                    {
                        Inline = false, 
                        Name = "Content", 
                        Value = e.Message.Content
                    });
                    footer = new EmbedFooter {Text = $"Author: {e.Message.Author.Id} | Message Id: {e.Message.Id}"};
                }
                else
                {
                    fields.Add(new EmbedField
                    {
                        Inline = false, 
                        Name = "Content", 
                        Value = "The value is not available due to the message was send while the bot were offline or it's no longer in the cache."
                    });
                    footer = new EmbedFooter {Text = $"Message Id: {e.Message.Id}"};
                }

                var embed = new Embed
                {
                    Title = ":wastebasket: Message deleted",
                    Description = new StringBuilder().AppendLine($"Message ({e.Message.Id}) deleted in {e.Channel.Mention}.")
                                                     .ToString(),
                    Color = DiscordColor.IndianRed,
                    ThumbnailUrl = thumbnailUrl,
                    Fields = fields,
                    Footer = footer
                };

                logChannel.SendEmbedMessageAsync(embed);
            }

            return Task.CompletedTask;
        }

        private Task MessagesBulkDeleted(MessageBulkDeleteEventArgs e)
        {
            if (!e.Channel.IsPrivate)
            {
                var log = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                               .GetAwaiter()
                               .GetResult()
                               .Logs.FirstOrDefault(x => x.LogType == LogType.Message);

                if (log != null)
                {
                    var logChannel = e.Guild.GetChannel(log.ChannelId);

                    var embed = new Embed
                    {
                        Title = ":KaidaErase: Message bulk deleted",
                        Description = new StringBuilder().AppendLine($"{e.Messages.Count} messages deleted in {e.Channel.Mention}.")
                                                         .ToString(),
                        Color = DiscordColor.IndianRed
                    };

                    logChannel.SendEmbedMessageAsync(embed, embedFooterStyle: EmbedFooterStyle.None);
                }
            }

            return Task.CompletedTask;
        }

        private Task MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

            if (e.Channel.IsPrivate)
            {
                logger.Information($"{e.User.GetUsertag()} ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the direct message.");

                return Task.CompletedTask;
            }

            logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            if (reactionService.IsListener(e.Guild.Id, e.Message.Id, e.Emoji))
            {
                reactionService.ManageRole(e.Message, e.Channel, (DiscordMember) e.User, e.Emoji);
            }

            return Task.CompletedTask;
        }

        private Task MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

            if (e.Channel.IsPrivate)
            {
                logger.Information($"{e.User.GetUsertag()} ({e.User.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the direct message.");

                return Task.CompletedTask;
            }

            logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task MessageReactionRemovedEmoji(MessageReactionRemoveEmojiEventArgs e)
        {
            logger.Information($"All reactions with the emoji '{e.Emoji.Name}' ({e.Emoji.Id}) of the message ({e.Message.Id}) has been removed in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task MessageReactionsCleared(MessageReactionsClearEventArgs e)
        {
            logger.Information($"All reactions of the message ({e.Message.Id}) has been cleared in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            logger.Information($"Voice server has been updated to '{e.Endpoint}' on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            // Todo: Log joined/left voice channel

            var action = string.Empty;

            if (e.Before.IsSelfDeafened != e.After.IsSelfDeafened)
            {
                action = $"'self deafened' from {e.Before.IsSelfDeafened.ToString()} to {e.After.IsSelfDeafened.ToString()}";
            }
            else if (e.Before.IsSelfMuted != e.After.IsSelfMuted)
            {
                action = $"'self muted' from {e.Before.IsSelfMuted.ToString()} to {e.After.IsSelfMuted.ToString()}";
            }
            else if (e.Before.IsServerDeafened != e.After.IsServerDeafened)
            {
                action = $"'server deafened' from {e.Before.IsServerDeafened.ToString()} to {e.After.IsServerDeafened.ToString()}";
            }
            else if (e.Before.IsServerMuted != e.After.IsServerMuted)
            {
                action = $"'server muted' from {e.Before.IsServerMuted.ToString()} to {e.After.IsServerMuted.ToString()}";
            }
            else if (e.Before.IsSuppressed != e.After.IsSuppressed)
            {
                action = $"'suppressed' from {e.Before.IsSuppressed.ToString()} to {e.After.IsSuppressed.ToString()}";
            }
            else
            {
                action = "'something' from a state to a new state";
            }

            logger.Information($"Voice state of the user '{e.User.GetUsertag()}' ({e.User.Id}) has been updated {action} in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task SocketOpened()
        {
            logger.Information("Socket has been opened.");

            return Task.CompletedTask;
        }

        private Task SocketClosed(SocketCloseEventArgs e)
        {
            logger.Warning($"Socket has been closed. [{e.CloseCode}] ({e.CloseMessage})");

            return Task.CompletedTask;
        }

        private Task SocketErrored(SocketErrorEventArgs e)
        {
            logger.Error($"Socket has been errored! ({e.Exception.Message})");

            return Task.CompletedTask;
        }

        private Task ClientErrored(ClientErrorEventArgs e)
        {
            switch (e.Exception)
            {
                case BadRequestException badRequestException:
                    logger.Error($"[{e.EventName}] Bad Request: {badRequestException.Message}");

                    break;
                case RateLimitException rateLimitException:
                    logger.Error($"[{e.EventName}] Rate Limit: {rateLimitException.Message}");

                    break;
                case UnauthorizedException unauthorizedException:
                    logger.Error($"[{e.EventName}] Unauthorized: {unauthorizedException.Message}");

                    break;
                case NotFoundException notFoundException:
                    logger.Error($"[{e.EventName}] Not Found: {notFoundException.WebResponse.Response}");

                    break;
            }

            logger.Error(e.Exception, $"Client has occurred an error: {e.EventName}");

            return Task.CompletedTask;
        }

        private Task UnknownEvent(UnknownEventArgs e)
        {
            logger.Warning($"Unknown Event: {e.EventName}");

            return Task.CompletedTask;
        }
    }
}