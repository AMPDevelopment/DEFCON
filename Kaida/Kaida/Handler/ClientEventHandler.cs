using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kaida.Library.Reaction;
using Serilog;

namespace Kaida.Handler
{
    public class ClientEventHandler
    {
        private readonly ILogger _logger;
        private readonly IReactionListener _reactionListener;
        private readonly DiscordShardedClient _client;
        
        public ClientEventHandler(DiscordShardedClient client, ILogger logger, IReactionListener reactionListener)
        {
            _client = client;
            _logger = logger;
            _reactionListener = reactionListener;

            _client.Ready += Ready;
            _client.GuildDownloadCompleted += GuildDownloadCompleted;
            _client.GuildAvailable += GuildAvailable;
            _client.GuildUnavailable += GuildUnavailable;
            _client.GuildMemberAdded += GuildMemberAdded;
            _client.GuildMemberRemoved += GuildMemberRemoved;
            _client.GuildCreated += GuildCreated;
            _client.GuildDeleted += GuildDeleted;
            _client.GuildBanAdded += GuildBanAdded;
            _client.GuildBanRemoved += GuildBanRemoved;
            _client.MessageReactionAdded += MessageReactionAdded;
            _client.MessageReactionRemoved += MessageReactionRemoved;
            _client.ClientErrored += ClientErrored;
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
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' has left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
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
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' has been banned from '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            _logger.Information($"'{e.Member.Username}#{e.Member.Discriminator}' has been unbanned from '{e.Guild.Name}' ({e.Guild.Id}).");
            return Task.CompletedTask;
        }

        private Task MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;
            _logger.Information($"'{e.User.Username}#{e.User.Discriminator}' has added the reaction '{e.Emoji.Name}' to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Channel.Guild.Name}' ({e.Channel.Guild.Id}).");
            if (!_reactionListener.IsListener(e.Message.Id, e.Emoji, e.Client)) return Task.CompletedTask;
            _reactionListener.GrantRole(e.Channel, e.Message, e.User, e.Emoji, e.Client);
            return Task.CompletedTask;
        }

        private Task MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;
            _logger.Information($"'{e.User.Username}#{e.User.Discriminator}' has revove the reaction '{e.Emoji.Name}' from the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Channel.Guild.Name}' ({e.Channel.Guild.Id}).");
            if (!_reactionListener.IsListener(e.Message.Id, e.Emoji, e.Client)) return Task.CompletedTask;
            _reactionListener.RevokeRole(e.Channel, e.Message, e.User, e.Emoji, e.Client);
            return Task.CompletedTask;
        }

        private Task ClientErrored(ClientErrorEventArgs e)
        {
            _logger.Error(e.Exception, $"Client has occurred an error: {e.EventName}");
            return Task.CompletedTask;
        }
    }
}
