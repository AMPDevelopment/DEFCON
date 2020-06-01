using System.Threading.Tasks;
using DSharpPlus.Entities;
using Kaida.Data.Roles;

namespace Kaida.Library.Services.Reactions
{
    public interface IReactionService
    {
        /// <summary>
        /// Checks if the reaction is associated to a <see cref="ReactionMenu"/> or <see cref="ReactionSingle"/>.
        /// </summary>
        /// <param name="guildId">The identity of the guild.</param>
        /// <param name="messageId">The identity of the message which it was reacted on.</param>
        /// <param name="emojiId">The identity of the emoji which was reacted.</param>
        /// <param name="client">The Discord client.</param>
        /// <returns></returns>
        bool IsListener(ulong guildId, ulong messageId, DiscordEmoji emoji);
        void ManageRole(DiscordMessage message, DiscordChannel channel, DiscordMember member, DiscordEmoji emoji);
        Task CreateReactionMenu(ulong guildId, ulong messageId, string name);
        Task AddRoleToListener(ulong guildId, ulong messageId, DiscordEmoji emoji, DiscordRole role, ReactionType type);
        Task RemoveRoleFromListener(ulong guildId, ulong messageId, DiscordEmoji emoji);
    }
}