using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis;

namespace Kaida.Library.Reaction
{
    public class ReactionListener : IReactionListener
    {
        private readonly List<int> _loadedListeners = new List<int>();
        private readonly ILogger _logger;
        private readonly IDatabase _redis;
        private Dictionary<string, Dictionary<DiscordEmoji, ulong>> _listener;

        public ReactionListener(ILogger logger, IDatabase redis)
        {
            _logger = logger;
            _redis = redis;
        }

        public bool IsListener(ulong id, DiscordEmoji emoji, BaseDiscordClient client)
        {
            CheckShardListener(client);

            return _listener.ContainsKey(id.ToString()) && _listener[id.ToString()].ContainsKey(emoji);
        }

        public Task AddRoleToListener(ulong messageId, DiscordEmoji emoji, DiscordRole role, BaseDiscordClient client)
        {
            if (_redis.SetMembers("MessageIds").All(e => e.ToString() != messageId.ToString()))
            {
                _redis.SetAdd("MessageIds", messageId);
            }

            _redis.HashSet($"Messages:{messageId}", new[] {new HashEntry(emoji.ToString(), role.Id.ToString())});

            if (_listener.ContainsKey(messageId.ToString()))
            {
                _listener[messageId.ToString()].Add(emoji, role.Id);

                return Task.CompletedTask;
            }

            var dictionary = new Dictionary<DiscordEmoji, ulong>
            {
                {
                    emoji,
                    role.Id
                }
            };

            _listener.Add(messageId.ToString(), dictionary);

            return Task.CompletedTask;
        }

        public Task RemoveRoleFromListener(ulong messageId, DiscordEmoji emoji, BaseDiscordClient client)
        {
            if (_listener.ContainsKey(messageId.ToString()) && _listener[messageId.ToString()].ContainsKey(emoji))
            {
                _redis.HashDelete($"Messages:{messageId}", emoji.ToString());
                _listener[messageId.ToString()].Remove(emoji);

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public async void ManageRole(DiscordChannel channel, DiscordMessage message, DiscordUser user, DiscordEmoji emoji, BaseDiscordClient client)
        {
            var roleId = _listener[message.Id.ToString()][emoji];
            var role = channel.Guild.GetRole(roleId);
            var member = await channel.Guild.GetMemberAsync(user.Id);

            if (member.Roles.Contains(role))
            {
                _logger.Information($"The role '{role.Name}' was revoked from '{user.Username}#{user.Discriminator}' on the guild '{channel.Guild.Name}' ({channel.Guild.Id}).");
                await ((DiscordMember) user).RevokeRoleAsync(role);
            }
            else
            {
                _logger.Information($"The role '{role.Name}' was granted to '{user.Username}#{user.Discriminator}' on the guild '{channel.Guild.Name}' ({channel.Guild.Id}).");
                await ((DiscordMember) user).GrantRoleAsync(role);
            }

            await message.DeleteReactionAsync(emoji, user);
        }

        public Task LoadListeners(BaseDiscordClient client)
        {
            var ids = _redis.SetMembers("MessageIds");
            _listener = new Dictionary<string, Dictionary<DiscordEmoji, ulong>>();

            foreach (var id in ids)
            {
                var reactions = _redis.HashGetAll($"Messages:{id}");
                var emojiDictionary = new Dictionary<DiscordEmoji, ulong>();

                foreach (var reaction in reactions)
                {
                    DiscordEmoji emoji;
                    var fieldName = reaction.Name.ToString();

                    if (fieldName.StartsWith("<"))
                    {
                        fieldName = fieldName.Substring(fieldName.LastIndexOf(':'));
                        var emojiId = fieldName.Substring(1, fieldName.Length - 2);
                        emoji = DiscordEmoji.FromGuildEmote(client, ulong.Parse(emojiId));
                    }
                    else
                    {
                        emoji = DiscordEmoji.FromUnicode(fieldName);
                    }

                    emojiDictionary.Add(emoji, ulong.Parse(reaction.Value));
                }

                _listener.Add(id, emojiDictionary);
            }

            return Task.CompletedTask;
        }

        private Task CheckShardListener(BaseDiscordClient client)
        {
            var shard = (DiscordClient) client;

            if (_loadedListeners.Contains(shard.ShardId)) return Task.CompletedTask;
            _loadedListeners.Add(shard.ShardId);
            LoadListeners(client);

            return Task.CompletedTask;
        }
    }
}