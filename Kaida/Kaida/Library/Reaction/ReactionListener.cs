using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using StackExchange.Redis;

namespace Kaida.Library.Reaction
{
    public class ReactionListener : IReactionListener
    {
        private readonly IDatabase _redis;
        private Dictionary<string, Dictionary<DiscordEmoji, ulong>> _listener;

        public ReactionListener(IDatabase redis)
        {
            _redis = redis;
        }

        public bool IsListener(ulong id, BaseDiscordClient client)
        {
            LoadListeners(client);
            return _listener.ContainsKey(id.ToString());
        }

        public Task AddRoleToListener(string messageId, DiscordEmoji emoji, DiscordRole role, BaseDiscordClient client)
        {
            LoadListeners(client);
            if (_redis.SetMembers("MessageIds").All(e => e.ToString() != messageId))
            {
                _redis.SetAdd("MessageIds", messageId);
            }

            _redis.HashSet($"Messages:{messageId}", new[] { new HashEntry(emoji.ToString(), role.Id.ToString()) });
            _redis.SetAdd("MessageIds", messageId);

            if (_listener.ContainsKey(messageId))
            {
                _listener[messageId].Add(emoji, role.Id);
                return Task.CompletedTask;
            }

            var dictionary = new Dictionary<DiscordEmoji, ulong>
            {
                {
                    emoji,
                    role.Id
                }
            };

            _listener.Add(messageId, dictionary);

            return Task.CompletedTask;
        }

        public async void RevokeRole(DiscordChannel channel, DiscordMessage message, DiscordUser user, DiscordEmoji emoji, BaseDiscordClient client)
        {
            var roleId = _listener[message.Id.ToString()][emoji];
            var role = channel.Guild.GetRole(roleId);
            await ((DiscordMember)user).RevokeRoleAsync(role);
        }

        public async void GrantRole(DiscordChannel channel, DiscordMessage message, DiscordUser user, DiscordEmoji emoji, BaseDiscordClient client)
        {
            var roleId = _listener[message.Id.ToString()][emoji];
            var role = channel.Guild.GetRole(roleId);
            await ((DiscordMember)user).GrantRoleAsync(role);
        }

        private Task LoadListeners(BaseDiscordClient client)
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
    }
}
