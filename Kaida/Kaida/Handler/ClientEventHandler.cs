using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Extensions;
using Kaida.Library.Reaction;
using MoreLinq.Extensions;
using Serilog;
using StackExchange.Redis;

namespace Kaida.Handler
{
    public class ClientEventHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly IReactionListener _reactionListener;
        private readonly ILogger logger;
        private readonly IDatabase redis;
        private int _activityIndex;
        private Timer _timer;

        public ClientEventHandler(DiscordShardedClient client, ILogger logger, IDatabase redis, IReactionListener reactionListener)
        {
            _client = client;
            this.logger = logger;
            this.redis = redis;
            _reactionListener = reactionListener;

            this.logger.Information("Register events...");
            _client.Ready += Ready;
            _client.GuildDownloadCompleted += GuildDownloadCompleted;
            _client.GuildAvailable += GuildAvailable;
            _client.GuildUnavailable += GuildUnavailable;
            _client.GuildCreated += GuildCreated;
            _client.GuildUpdated += GuildUpdated;
            _client.GuildDeleted += GuildDeleted;
            _client.GuildEmojisUpdated += GuildEmojisUpdated;
            _client.GuildIntegrationsUpdated += GuildIntegrationsUpdated;
            _client.ChannelCreated += ChannelCreated;
            _client.ChannelDeleted += ChannelDeleted;
            _client.ChannelUpdated += ChannelUpdated;
            _client.GuildRoleCreated += GuildRoleCreated;
            _client.GuildRoleUpdated += GuildRoleUpdated;
            _client.GuildRoleDeleted += GuildRoleDeleted;
            _client.GuildMemberAdded += GuildMemberAdded;
            _client.GuildMemberRemoved += GuildMemberRemoved;
            _client.GuildMemberUpdated += GuildMemberUpdated;
            _client.GuildBanAdded += GuildBanAdded;
            _client.GuildBanRemoved += GuildBanRemoved;
            _client.DmChannelCreated += DmChannelCreated;
            _client.DmChannelDeleted += DmChannelDeleted;
            _client.MessageCreated += MessageCreated;
            _client.MessageUpdated += MessageUpdated;
            _client.MessageDeleted += MessageDeleted;
            _client.MessagesBulkDeleted += MessagesBulkDeleted;
            _client.MessageReactionAdded += MessageReactionAdded;
            _client.MessageReactionRemoved += MessageReactionRemoved;
            _client.MessageReactionRemovedEmoji += MessageReactionRemovedEmoji;
            _client.MessageReactionsCleared += MessageReactionsCleared;
            _client.VoiceServerUpdated += VoiceServerUpdated;
            _client.VoiceStateUpdated += VoiceStateUpdated;
            _client.SocketOpened += SocketOpened;
            _client.SocketClosed += SocketClosed;
            _client.SocketErrored += SocketErrored;
            _client.ClientErrored += ClientErrored;
            _client.UnknownEvent += UnknownEvent;
            this.logger.Information("Registered all events!");
        }

        private async Task Ready(ReadyEventArgs e)
        {
            logger.Information("Client is ready!");

            _timer = new Timer(async _ =>
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

                var activities = new List<DiscordActivity> {new DiscordActivity {ActivityType = ActivityType.Watching, Name = $"{guildsCount} servers"}, new DiscordActivity {ActivityType = ActivityType.ListeningTo, Name = $"{uniqueUsers} unique users"}};
                await _client.UpdateStatusAsync(activities.ElementAtOrDefault(_activityIndex), UserStatus.Online, DateTimeOffset.UtcNow);
                _activityIndex = _activityIndex + 1 == activities.Count ? 0 : _activityIndex + 1;
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
        }

        private Task GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
        {
            logger.Information("Client downloaded all guilds successfully.");

            return Task.CompletedTask;
        }

        private Task GuildAvailable(GuildCreateEventArgs e)
        {
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

            return Task.CompletedTask;
        }

        private Task GuildUpdated(GuildUpdateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildDeleted(GuildDeleteEventArgs e)
        {
            logger.Information($"Left the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildEmojisUpdated(GuildEmojisUpdateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildIntegrationsUpdated(GuildIntegrationsUpdateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task ChannelCreated(ChannelCreateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task ChannelUpdated(ChannelUpdateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task ChannelDeleted(ChannelDeleteEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            var joinedLeftEventId = redis.StringGet($"{e.Guild.Id}:Logs:JoinedLeftEvent");

            var guild = e.Guild;
            var member = e.Member;

            if (!string.IsNullOrWhiteSpace(joinedLeftEventId))
            {
                var logChannel = guild.GetChannel((ulong) joinedLeftEventId);

                var embed = new Embed
                {
                    Title = ":inbox_tray: Member joined",
                    Description = new StringBuilder().AppendLine($"Username: `{member.GetUsertag()}`")
                                                     .AppendLine($"User identity: `{member.Id}`")
                                                     .AppendLine($"Registered: {member.CreatedAtLongDateTimeString().Result}")
                                                     .ToString(),
                    Color = DiscordColor.SpringGreen,
                    ThumbnailUrl = member.AvatarUrl,
                    Footer = new EmbedFooter {Text = $"Member Id: {member.Id}"}
                };

                logChannel.SendEmbedMessageAsync(embed);
            }

            logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            var joinedLeftEventId = redis.StringGet($"{e.Guild.Id}:Logs:JoinedLeftEvent");

            var guild = e.Guild;
            var member = e.Member;

            if (!string.IsNullOrWhiteSpace(joinedLeftEventId))
            {
                var logChannel = guild.GetChannel((ulong) joinedLeftEventId);

                var roles = member.Roles.Any()
                    ? member.Roles.Where(x => x.Name != "@everyone")
                            .OrderByDescending(r => r.Position)
                            .Aggregate("", (current, x) => current + $"{x.Mention} ")
                    : "None";

                var fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Roles", Value = roles}};

                var embed = new Embed
                {
                    Title = ":outbox_tray: Member left",
                    Description = new StringBuilder().AppendLine($"Username: `{member.GetUsertag()}`")
                                                     .AppendLine($"User identity: `{member.Id}`")
                                                     .ToString(),
                    Color = DiscordColor.IndianRed,
                    ThumbnailUrl = member.AvatarUrl,
                    Fields = fields,
                    Footer = new EmbedFooter {Text = $"Member Id: {member.Id}"}
                };

                logChannel.SendEmbedMessageAsync(embed);
            }

            logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has left the guild '{e.Guild.Name}' ({e.Guild.Id}).");

            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            var before = string.IsNullOrWhiteSpace(e.NicknameBefore) ? e.Member.Username : e.NicknameBefore;
            var after = string.IsNullOrWhiteSpace(e.NicknameAfter) ? e.Member.Username : e.NicknameAfter;

            if (before == after) return Task.CompletedTask;

            var nicknameEventId = redis.StringGet($"{e.Guild.Id}:Logs:NicknameEvent");

            if (!string.IsNullOrWhiteSpace(nicknameEventId))
            {
                var channel = e.Guild.GetChannel((ulong) nicknameEventId);

                var fields = new List<EmbedField> {new EmbedField {Inline = false, Name = "Before", Value = before}, new EmbedField {Inline = false, Name = "After", Value = after}};

                var embed = new Embed
                {
                    Title = ":memo: Nickname changed",
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
            /* This would kill my bot */
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

            var messageEventId = redis.StringGet($"{e.Guild.Id}:Logs:MessageEvent");
            var logChannel = e.Guild.GetChannel((ulong) messageEventId);

            if (!string.IsNullOrWhiteSpace(messageEventId))
            {
                var contentNow = e.Message.Content;
                var fields = new List<EmbedField>();

                if (e.MessageBefore != null)
                {
                    var contentBefore = e.MessageBefore.Content;

                    if (contentBefore.Equals(contentNow)) return Task.CompletedTask;

                    fields.Add(new EmbedField {Inline = false, Name = "Before", Value = contentBefore});
                }
                else
                {
                    fields.Add(new EmbedField {Inline = false, Name = "Before", Value = "The value is not available due to the message was send while the bot were offline or it's no longer in the cache."});
                }

                fields.Add(new EmbedField {Inline = false, Name = "After", Value = contentNow});

                var embed = new Embed
                {
                    Title = ":memo: Message updated",
                    Description = new StringBuilder().AppendLine($"Message ({e.Message.Id}) updated in {e.Channel.Mention}.")
                                                     .AppendLine($"[Jump to message]({e.Message.JumpLink})")
                                                     .ToString(),
                    Color = DiscordColor.Orange,
                    ThumbnailUrl = e.Author.AvatarUrl,
                    Fields = fields,
                    Footer = new EmbedFooter {Text = $"Author: {e.Author.Id} | Message Id: {e.Message.Id}"}
                };

                logChannel.SendEmbedMessageAsync(embed);

                logger.Information($"The message ({e.Message.Id}) from '{e.Author.GetUsertag()}' ({e.Author.Id}) was updated in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
            }

            return Task.CompletedTask;
        }

        private Task MessageDeleted(MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
            {
                logger.Information(!string.IsNullOrWhiteSpace(e.Message.Content) ? $"The message ({e.Message.Id}) from '{e.Message.Author.GetUsertag()}' ({e.Message.Author.Id}) was deleted in the direct message." : $"The message ({e.Message.Id}) was deleted in the direct message.");

                return Task.CompletedTask;
            }

            var guildPrefix = redis.StringGet($"{e.Guild.Id}:CommandPrefix");
            var messageEventId = redis.StringGet($"{e.Guild.Id}:Logs:MessageEvent");
            var logChannel = e.Guild.GetChannel((ulong) messageEventId);

            var thumbnailUrl = string.Empty;
            var fields = new List<EmbedField>();
            EmbedFooter footer;

            if (!string.IsNullOrWhiteSpace(e.Message.Content))
            {
                if (e.Message.Author.IsBot) return Task.CompletedTask;

                if (e.Message.Content.StartsWith(guildPrefix)) return Task.CompletedTask;

                thumbnailUrl = e.Message.Author.AvatarUrl;
                fields.Add(new EmbedField {Inline = false, Name = "Content", Value = e.Message.Content});
                footer = new EmbedFooter {Text = $"Author: {e.Message.Author.Id} | Message Id: {e.Message.Id}"};
                logger.Information($"The message ({e.Message.Id}) from '{e.Message.Author.GetUsertag()}' ({e.Message.Author.Id}) was deleted in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
            }
            else
            {
                fields.Add(new EmbedField {Inline = false, Name = "Content", Value = "The value is not available due to the message was send while the bot were offline or it's no longer in the cache."});
                footer = new EmbedFooter {Text = $"Message Id: {e.Message.Id}"};
                logger.Information($"The message ({e.Message.Id}) was deleted in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
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

            return Task.CompletedTask;
        }

        private Task MessagesBulkDeleted(MessageBulkDeleteEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            var client = e.Client;
            var guild = e.Guild;
            var channel = e.Channel;
            var message = e.Message;
            var reactionUser = e.User;
            var emoji = e.Emoji;
            var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

            if (channel.IsPrivate)
            {
                logger.Information($"{reactionUser.GetUsertag()} ({reactionUser.Id}) has added {emojiName} to the message '{e.Message.Id}' in the direct message.");

                return Task.CompletedTask;
            }

            logger.Information($"'{reactionUser.GetUsertag()}' ({reactionUser.Id}) has added {emojiName} to the message '{message.Id}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");

            if (!_reactionListener.IsListener(message.Id, emoji, client)) return Task.CompletedTask;

            _reactionListener.ManageRole(channel, message, reactionUser, emoji, client);

            return Task.CompletedTask;
        }

        private Task MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            var guild = e.Guild;
            var channel = e.Channel;
            var message = e.Message;
            var reactionUser = e.User;
            var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

            if (channel.IsPrivate)
            {
                logger.Information($"{reactionUser.GetUsertag()} ({reactionUser.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the direct message.");

                return Task.CompletedTask;
            }

            logger.Information($"'{reactionUser.GetUsertag()}' ({reactionUser.Id}) has removed {emojiName} to the message '{message.Id}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");

            return Task.CompletedTask;
        }

        private Task MessageReactionRemovedEmoji(MessageReactionRemoveEmojiEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task MessageReactionsCleared(MessageReactionsClearEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task SocketOpened()
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task SocketClosed(SocketCloseEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task SocketErrored(SocketErrorEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }

        private Task ClientErrored(ClientErrorEventArgs e)
        {
            logger.Error(e.Exception, $"Client has occurred an error: {e.EventName}");

            return Task.CompletedTask;
        }

        private Task UnknownEvent(UnknownEventArgs e)
        {
            /* This would kill my bot */
            return Task.CompletedTask;
        }
    }
}