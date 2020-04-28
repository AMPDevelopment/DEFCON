using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kaida.Entities.Discord;
using Kaida.Library.Extensions;
using Kaida.Library.Reaction;
using Serilog;
using StackExchange.Redis;

namespace Kaida.Handler
{
    public class ClientEventHandler
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;
        private readonly IReactionListener _reactionListener;
        private readonly DiscordShardedClient _client;
        private Timer _timer;
        private int _activityIndex = 0;

        public ClientEventHandler(DiscordShardedClient client, ILogger logger, IDatabase redis, IReactionListener reactionListener)
        {

            _client = client;
            _logger = logger;
            _redis = redis;
            _reactionListener = reactionListener;

            _logger.Information("Register events...");
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
            _logger.Information("Registered all events!");
        }

        private Task Ready(ReadyEventArgs e)
        {
            _logger.Information("Client is ready!");
            
            _timer = new Timer(async _ =>
            {
                var guilds = e.Client.Guilds.Values.ToList();
                var guildsCount = guilds.Count;
                var totalUsers = new List<DiscordUser>();
                var totalBots = new List<DiscordUser>();

                foreach (var guild in guilds)
                {
                    totalUsers.AddRange(guild.GetAllMembersAsync().Result.Where(x => x.IsBot == false));
                    totalBots.AddRange(guild.GetAllMembersAsync().Result.Where(x => x.IsBot == true));
                }

                var uniqueUsers = totalUsers.Distinct().ToList().Count;
                var uniqueBots = totalBots.Distinct().ToList().Count;

                var activities = new List<DiscordActivity>()
                {
                    new DiscordActivity()
                    {
                        ActivityType = ActivityType.Watching,
                        Name = $"{guildsCount} servers"
                    },
                    new DiscordActivity()
                    {
                        ActivityType = ActivityType.ListeningTo,
                        Name = $"{uniqueUsers} unique users"
                    }
                };
                await _client.UpdateStatusAsync(activities.ElementAtOrDefault(_activityIndex), UserStatus.Online, DateTimeOffset.UtcNow);
                _activityIndex = _activityIndex + 1 == activities.Count ? 0 : _activityIndex + 1;
            },
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        private Task GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
        {
            _logger.Information("Client downloaded all guilds successfully.");
            return Task.CompletedTask;
        }

        private Task GuildAvailable(GuildCreateEventArgs e)
        {
            _logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became available.");
            return Task.CompletedTask;
        }

        private Task GuildUnavailable(GuildDeleteEventArgs e)
        {
            _logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became unavailable.");
            return Task.CompletedTask;
        }

        private Task GuildCreated(GuildCreateEventArgs e)
        {
            _logger.Information($"Joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildUpdated(GuildUpdateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildDeleted(GuildDeleteEventArgs e)
        {
            _logger.Information($"Left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildEmojisUpdated(GuildEmojisUpdateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildIntegrationsUpdated(GuildIntegrationsUpdateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task ChannelCreated(ChannelCreateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task ChannelUpdated(ChannelUpdateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task ChannelDeleted(ChannelDeleteEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            var joinedLeftEventId = _redis.StringGet($"{e.Guild.Id}:Logs:JoinedLeftEvent");

            var guild = e.Guild;
            var member = e.Member;

            if (!string.IsNullOrWhiteSpace(joinedLeftEventId))
            {
                var logChannel = guild.GetChannel((ulong)joinedLeftEventId);
                var avatarUrl = member.AvatarUrl;
                var title = ":inbox_tray: Member joined";
                var description = new StringBuilder()
                    .AppendLine($"Username: `{member.GetUsertag()}`")
                    .AppendLine($"User identity: `{member.Id}`")
                    .AppendLine($"Registered: {member.CreatedAtLongDateTimeString().Result}").ToString();

                var footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Member Id: {member.Id}"
                };

                logChannel.SendEmbedMessageAsync(title: title, description: description, color: DiscordColor.SpringGreen, thumbnailUrl: avatarUrl, footer: footer, timestamp: DateTimeOffset.UtcNow);
            }

            _logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            var joinedLeftEventId = _redis.StringGet($"{e.Guild.Id}:Logs:JoinedLeftEvent");

            var guild = e.Guild;
            var member = e.Member;

            if (!string.IsNullOrWhiteSpace(joinedLeftEventId))
            {
                var logChannel = guild.GetChannel((ulong)joinedLeftEventId);
                var avatarUrl = member.AvatarUrl;
                var title = ":outbox_tray: Member left";
                var description = new StringBuilder()
                    .AppendLine($"Username: `{member.GetUsertag()}`")
                    .AppendLine($"User identity: `{member.Id}`").ToString();

                var roles = string.Empty;
                if (member.Roles.Count() > 0)
                {
                    var rolesSorted = member.Roles.ToList().OrderByDescending(x => x.Position);

                    foreach (var role in rolesSorted)
                    {
                        roles += $"<@&{role.Id}> ";
                    }
                }
                else
                {
                    roles = "None";
                }

                var fields = new List<EmbedField>()
                {
                    new EmbedField()
                    {
                        Inline = false,
                        Name = "Roles",
                        Value = roles
                    }
                };

                var footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Member Id: {member.Id}"
                };

                logChannel.SendEmbedMessageAsync(title: title, description: description, color: DiscordColor.IndianRed, fields: fields, thumbnailUrl: avatarUrl, footer: footer, timestamp: DateTimeOffset.UtcNow);
            }

            _logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            var before = string.IsNullOrWhiteSpace(e.NicknameBefore) == true ? e.Member.Username : e.NicknameBefore;
            var after = string.IsNullOrWhiteSpace(e.NicknameAfter) == true ? e.Member.Username : e.NicknameAfter;

            if (before != after)
            {
                var nicknameEventId = _redis.StringGet($"{e.Guild.Id}:Logs:NicknameEvent");

                if (!string.IsNullOrWhiteSpace(nicknameEventId))
                {
                    var channel = e.Guild.GetChannel((ulong)nicknameEventId);
                    var avatarUrl = e.Member.AvatarUrl;
                    var title = ":memo: Nickname changed";
                    var description = new StringBuilder()
                        .AppendLine($"Username: {e.Member.GetUsertag()}")
                        .AppendLine($"Identity: {e.Member.Id}").ToString();

                    var fields = new List<EmbedField>(){
                        new EmbedField()
                        {
                            Inline = false,
                            Name = "Before",
                            Value = before
                        },
                        new EmbedField()
                        {
                            Inline = false,
                            Name = "After",
                            Value = after
                        }
                    };

                    channel.SendEmbedMessageAsync(title: title, description: description, color: DiscordColor.CornflowerBlue, fields: fields, thumbnailUrl: avatarUrl, timestamp: DateTimeOffset.UtcNow);
                }
                _logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has changed there nickname from '{before}' to '{after}' on '{e.Guild.Name}' ({e.Guild.Id}).");
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private Task GuildBanAdded(GuildBanAddEventArgs e)
        {
            _logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been banned from '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            _logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been unbanned from '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task DmChannelCreated(DmChannelCreateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task DmChannelDeleted(DmChannelDeleteEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task MessageCreated(MessageCreateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task MessageUpdated(MessageUpdateEventArgs e)
        {
            if (e.Message.Author.IsBot) return Task.CompletedTask;

            var messageEventId = _redis.StringGet($"{e.Guild.Id}:Logs:MessageEvent");
            var guild = e.Guild;
            var channel = e.Channel;
            var message = e.Message;
            var messageAuthor = message.Author;
            var member = guild.GetMemberAsync(messageAuthor.Id).Result;

            if (channel.IsPrivate)
            {
                _logger.Information($"The message ({message.Id}) from '{messageAuthor.GetUsertag()}' ({messageAuthor.Id}) was updated in the direct message.");
                return Task.CompletedTask;
            }

            if (!string.IsNullOrWhiteSpace(messageEventId))
            {
                var logChannel = guild.GetChannel((ulong)messageEventId);
                if (channel != logChannel)
                {
                    var avatarUrl = messageAuthor.AvatarUrl;
                    var title = ":memo: Message updated";
                    var description = new StringBuilder()
                        .AppendLine($"Message sent by {messageAuthor.Mention} updated in {channel.Mention}.")
                        .AppendLine($"[Jump to message]({message.JumpLink.ToString()})").ToString();

                    var contentBefore = e.MessageBefore.Content;
                    var contentNow = message.Content;

                    var fields = new List<EmbedField>(){
                    new EmbedField()
                    {
                        Inline = false,
                        Name = "Before",
                        Value = contentBefore
                    },
                    new EmbedField()
                    {
                        Inline = false,
                        Name = "After",
                        Value = contentNow
                    }
                };

                    var footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        Text = $"Author: {messageAuthor.Id} | Message Id: {message.Id}"
                    };

                    logChannel.SendEmbedMessageAsync(title: title, description: description, color: DiscordColor.Orange, fields: fields, thumbnailUrl: avatarUrl, footer: footer, timestamp: DateTimeOffset.UtcNow);
                }
            }

            _logger.Information($"The message ({message.Id}) from '{messageAuthor.GetUsertag()}' ({messageAuthor.Id}) was updated in '{channel.Name}' ({channel.Id}) on '{guild.Name}' ({guild.Id}).");
            return Task.CompletedTask;
        }

        private Task MessageDeleted(MessageDeleteEventArgs e)
        {
            if (e.Message.Author.IsBot) return Task.CompletedTask;

            var guildPrefix = _redis.StringGet($"{e.Guild.Id}:CommandPrefix");
            var messageEventId = _redis.StringGet($"{e.Guild.Id}:Logs:MessageEvent");
            var guild = e.Guild;
            var channel = e.Channel;
            var message = e.Message;
            var messageAuthor = message.Author;
            var member = e.Guild.GetMemberAsync(messageAuthor.Id).Result;
            var deletedMessage = message.Content;

            if (channel.IsPrivate)
            {
                _logger.Information($"The message ({message.Id}) from '{messageAuthor.GetUsertag()}' ({messageAuthor.Id}) was updated in the direct message.");
                return Task.CompletedTask;
            }

            if (!deletedMessage.StartsWith(guildPrefix))
            {
                if (!string.IsNullOrWhiteSpace(messageEventId))
                {
                    var logChannel = guild.GetChannel((ulong)messageEventId);
                    if (channel != logChannel)
                    {
                        var attachmentCount = message.Attachments.Count;
                        var avatarUrl = e.Message.Author.AvatarUrl;
                        var title = ":wastebasket: Message deleted";
                        var description = $"Message sent by {messageAuthor.Mention} deleted in {channel.Mention}.";
                        var singleImage = string.Empty;

                        var fields = new List<EmbedField>(){
                        new EmbedField()
                        {
                            Inline = false,
                            Name = "Content",
                            Value = deletedMessage
                        }
                    };

                        var footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = $"Author: {messageAuthor.Id} | Message Id: {message.Id}"
                        };

                        if (!attachmentCount.Equals(0))
                        {
                            var attachments = new Dictionary<string, Stream>();
                        }
                        else
                        {
                            logChannel.SendEmbedMessageAsync(title: title, description: description, color: DiscordColor.IndianRed, fields: fields, thumbnailUrl: avatarUrl, image: singleImage, footer: footer, timestamp: DateTimeOffset.UtcNow);
                        }
                    }
                }
            }

            _logger.Information($"The message ({message.Id}) from '{messageAuthor.GetUsertag()}' ({messageAuthor.Id}) was deleted in '{channel.Name}' ({channel.Id}) on '{guild.Name}' ({guild.Id}).");
            return Task.CompletedTask;
        }

        private Task MessagesBulkDeleted(MessageBulkDeleteEventArgs e)
        {
            /* Bruh this would kill my bot */
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
            var emojiName = e.Emoji.Name.ToString() == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

            if (channel.IsPrivate)
            {
                _logger.Information($"{reactionUser.GetUsertag()} ({reactionUser.Id}) has added {emojiName} to the message '{e.Message.Id}' in the direct message.");
                return Task.CompletedTask;
            }

            _logger.Information($"'{reactionUser.GetUsertag()}' ({reactionUser.Id}) has added {emojiName} to the message '{message.Id}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");
            if (!_reactionListener.IsListener(message.Id, emoji, client)) return Task.CompletedTask;

            _reactionListener.ManageRole(channel, message, reactionUser, emoji, client);
            return Task.CompletedTask;
        }

        private Task MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            var client = e.Client;
            var guild = e.Guild;
            var channel = e.Channel;
            var message = e.Message;
            var reactionUser = e.User;
            var emoji = e.Emoji;
            var emojiName = e.Emoji.Name.ToString() == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

            if (channel.IsPrivate)
            {
                _logger.Information($"{reactionUser.GetUsertag()} ({reactionUser.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the direct message.");
                return Task.CompletedTask;
            }

            _logger.Information($"'{reactionUser.GetUsertag()}' ({reactionUser.Id}) has removed {emojiName} to the message '{message.Id}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");
            return Task.CompletedTask;
        }

        private Task MessageReactionRemovedEmoji(MessageReactionRemoveEmojiEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task MessageReactionsCleared(MessageReactionsClearEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task SocketOpened()
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task SocketClosed(SocketCloseEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task SocketErrored(SocketErrorEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }

        private Task ClientErrored(ClientErrorEventArgs e)
        {
            _logger.Error(e.Exception, $"Client has occurred an error: {e.EventName}");
            return Task.CompletedTask;
        }

        private Task UnknownEvent(UnknownEventArgs e)
        {
            /* Bruh this would kill my bot */
            return Task.CompletedTask;
        }
    }
}
