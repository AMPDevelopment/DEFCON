using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        public ClientEventHandler(DiscordShardedClient client, ILogger logger, IDatabase redis, IReactionListener reactionListener)
        {
            _client = client;
            _logger = logger;
            _redis = redis;
            _reactionListener = reactionListener;

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
        }

        private Task Ready(ReadyEventArgs e)
        {
            _logger.Information("Client is ready!");
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
            _logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
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
                    var description = new StringBuilder()
                        .AppendLine($"Username: {e.Member.GetUsertag()}")
                        .AppendLine($"Identity: {e.Member.Id}").ToString();

                    var author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        IconUrl = avatarUrl,
                        Name = "Nickname changed"
                    };

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

                    channel.SendEmbedMessageAsync(description: description, color: DiscordColor.Orange, author: author, fields: fields, thumbnailUrl: avatarUrl, timestamp: DateTimeOffset.UtcNow);
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
            if (e.Message.Author.IsCurrent || e.Message.Author.IsBot) return Task.CompletedTask;

            var messageEventId = _redis.StringGet($"{e.Guild.Id}:Logs:MessageEvent");
            var message = e.Message;
            var member = e.Guild.GetMemberAsync(message.Author.Id).Result;

            if (!string.IsNullOrWhiteSpace(messageEventId))
            {
                var logChannel = e.Guild.GetChannel((ulong)messageEventId);
                var avatarUrl = e.Message.Author.AvatarUrl;
                var title = ":memo: Message updated";
                var description = new StringBuilder()
                    .AppendLine($"Message sent by {message.Author.Mention} updated in {message.Channel.Mention}.")
                    .AppendLine($"[Jump to message]({message.JumpLink.ToString()})").ToString();
                var before = e.MessageBefore.Content;
                var after = message.Content;

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

                var footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Author: {message.Author.Id} | Message Id: {message.Id}"
                };

                logChannel.SendEmbedMessageAsync(title: title, description: description, color: DiscordColor.Orange, fields: fields, thumbnailUrl: avatarUrl, footer: footer, timestamp: DateTimeOffset.UtcNow);
            }
            _logger.Information($"The message ({message.Id}) from '{message.Author.GetUsertag()}' ({message.Author.Id}) was updated in '{message.Channel.Name}' ({message.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task MessageDeleted(MessageDeleteEventArgs e)
        {
            if (e.Message.Author.IsCurrent || e.Message.Author.IsBot) return Task.CompletedTask;

            var messageEventId = _redis.StringGet($"{e.Guild.Id}:Logs:MessageEvent");
            var message = e.Message;
            var member = e.Guild.GetMemberAsync(message.Author.Id).Result;
            var deletedMessage = message.Content;

            if (!string.IsNullOrWhiteSpace(messageEventId))
            {
                var logChannel = e.Guild.GetChannel((ulong)messageEventId);
                var attachmentCount = message.Attachments.Count;
                var avatarUrl = e.Message.Author.AvatarUrl;
                var title = ":wastebasket: Message deleted";
                var description = $"Message sent by {message.Author.Mention} deleted in {message.Channel.Mention}.";
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
                    Text = $"Author: {message.Author.Id} | Message Id: {message.Id}"
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
            _logger.Information($"The message ({message.Id}) from '{message.Author.GetUsertag()}' ({message.Author.Id}) was deleted in '{message.Channel.Name}' ({message.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
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
            var emojiName = e.Emoji.Name.ToString() == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";
            _logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Channel.Guild.Name}' ({e.Channel.Guild.Id}).");
            if (!_reactionListener.IsListener(e.Message.Id, e.Emoji, e.Client)) return Task.CompletedTask;
            _reactionListener.ManageRole(e.Channel, e.Message, e.User, e.Emoji, e.Client);
            return Task.CompletedTask;
        }

        private Task MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;
            var emojiName = e.Emoji.Name.ToString() == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";
            _logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has revoked {emojiName} from the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Channel.Guild.Name}' ({e.Channel.Guild.Id}).");
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
