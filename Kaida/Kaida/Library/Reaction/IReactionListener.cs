using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Kaida.Library.Reaction
{
    public interface IReactionListener
    {
        bool IsListener(ulong id, DiscordEmoji emoji, BaseDiscordClient client);
        Task AddRoleToListener(ulong messageId, DiscordEmoji emoji, DiscordRole role, BaseDiscordClient client);
        Task RemoveRoleFromListener(ulong messageId, DiscordEmoji emoji, BaseDiscordClient client);
        void ManageRole(DiscordChannel channel, DiscordMessage message, DiscordUser user, DiscordEmoji emoji, BaseDiscordClient client);
        Task LoadListeners(BaseDiscordClient client);
    }
}
