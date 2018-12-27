using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Kaida.Library.Reaction
{
    public interface IReactionListener
    {
        bool IsListener(ulong id, BaseDiscordClient client);
        Task AddRoleToListener(string messageId, DiscordEmoji emoji, DiscordRole role, BaseDiscordClient client);
        void RevokeRole(DiscordChannel channel, DiscordMessage message, DiscordUser user, DiscordEmoji emoji, BaseDiscordClient client);
        void GrantRole(DiscordChannel channel, DiscordMessage message, DiscordUser user, DiscordEmoji emoji, BaseDiscordClient client);
    }
}
