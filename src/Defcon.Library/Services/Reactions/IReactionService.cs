using System.Threading.Tasks;
using Defcon.Data.Roles;
using DSharpPlus.Entities;

namespace Defcon.Library.Services.Reactions
{
    public interface IReactionService
    {
        /// <summary>
        /// Checks if the reaction is associated to a <see cref="ReactionMenu"/> or <see cref="ReactionCategory"/>.
        /// </summary>
        /// <param name="guildId">The identity of the guild.</param>
        /// <param name="messageId">The identity of the message which it was reacted on.</param>
        /// <param name="emojiId">The identity of the emoji which was reacted.</param>
        /// <param name="client">The Discord client.</param>
        /// <returns></returns>
        bool IsListener(ulong guildId, ulong messageId, DiscordEmoji emoji);
        void ManageRole(DiscordMessage message, DiscordChannel channel, DiscordMember member, DiscordEmoji emoji);
        Task CreateReactionCategory(ulong guildId, ulong messageId, string name, ulong roleId);
        Task AddReactionListener(ulong guildId, ulong messageId, DiscordEmoji emoji, DiscordRole role, ReactionType type);
        Task RemoveRoleFromListener(ulong guildId, ulong messageId, DiscordEmoji emoji);
    }
}