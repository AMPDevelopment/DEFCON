using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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

        

        

        private Task VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task MessageReactionsCleared(MessageReactionsClearEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task MessageUpdated(MessageUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task MessageReactionRemovedEmoji(MessageReactionRemoveEmojiEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task MessagesBulkDeleted(MessageBulkDeleteEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task MessageDeleted(MessageDeleteEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task MessageCreated(MessageCreateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task DmChannelDeleted(DmChannelDeleteEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task DmChannelCreated(DmChannelCreateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task GuildIntegrationsUpdated(GuildIntegrationsUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task GuildEmojisUpdated(GuildEmojisUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task GuildUpdated(GuildUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task ChannelUpdated(ChannelUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task ChannelDeleted(ChannelDeleteEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task ChannelCreated(ChannelCreateEventArgs e)
        {
            throw new System.NotImplementedException();
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

        private Task GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' ({e.Member.Id}) has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' ({e.Member.Id}) has left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildCreated(GuildCreateEventArgs e)
        {
            _logger.Information($"Joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildDeleted(GuildDeleteEventArgs e)
        {
            _logger.Information($"Left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildBanAdded(GuildBanAddEventArgs e)
        {
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' ({e.Member.Id}) has been banned from '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' ({e.Member.Id}) has been unbanned from '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;
            var emojiName = e.Emoji.Name.ToString() == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";
            _logger.Information($"'{e.User.Username}#{e.User.Discriminator}' ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Channel.Guild.Name}' ({e.Channel.Guild.Id}).");
            if (!_reactionListener.IsListener(e.Message.Id, e.Emoji, e.Client)) return Task.CompletedTask;
            _reactionListener.ManageRole(e.Channel, e.Message, e.User, e.Emoji, e.Client);
            return Task.CompletedTask;
        }

        private Task MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;
            var emojiName = e.Emoji.Name.ToString() == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";
            _logger.Information($"'{e.User.Username}#{e.User.Discriminator}' ({e.User.Id}) has revoked {emojiName} from the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Channel.Guild.Name}' ({e.Channel.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task SocketOpened()
        {
            throw new System.NotImplementedException();
        }

        private Task SocketClosed(SocketCloseEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task SocketErrored(SocketErrorEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task ClientErrored(ClientErrorEventArgs e)
        {
            _logger.Error(e.Exception, $"Client has occurred an error: {e.EventName}");
            return Task.CompletedTask;
        }

        private Task UnknownEvent(UnknownEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
