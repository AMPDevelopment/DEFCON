using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Kaida.Library.Extensions
{
    public static class DiscordChannelExtension
    {
        /// <summary>
        ///     Deletes a list of <see cref="DiscordMessage" /> in the <see cref="DiscordChannel" />.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <param name="messages">Represents a list of <see cref="DiscordMessage" /> which will deleted.</param>
        /// <param name="reason">Represents the reason why those <see cref="DiscordMessage" />s has been deleted.</param>
        /// <returns></returns>
        public static async Task BulkMessagesAsync(this DiscordChannel channel, IEnumerable<DiscordMessage> messages, string reason = null)
        {
            await channel.DeleteMessagesAsync(messages, reason);
        }

        /// <summary>
        ///     Deletes a list of <see cref="DiscordMessage" /> from the <see cref="DiscordMember" /> in the
        ///     <see cref="DiscordChannel" />.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <param name="messages">Represents a list of <see cref="DiscordMessage" /> which will deleted.</param>
        /// <param name="member">
        ///     Represents the suspect <see cref="DiscordMember" /> which will all his
        ///     <see cref="DiscordMessage" />s
        /// </param>
        /// <param name="reason">Represents the reason why those <see cref="DiscordMessage" />s has been deleted.</param>
        /// <returns></returns>
        public static async Task BulkMessagesFromUserAsync(this DiscordChannel channel, IEnumerable<DiscordMessage> messages, DiscordMember member, string reason = null)
        {
            var memberMessage = messages.Where(x => x.Author == member)
                                        .ToList();
            await channel.DeleteMessagesAsync(memberMessage, reason);
        }

        /// <summary>
        ///     Deletes a single <see cref="DiscordMessage" /> by id.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <param name="messageId">Represents the id of the <see cref="DiscordMessage" />.</param>
        /// <returns></returns>
        public static async Task DeleteMessageByIdAsync(this DiscordChannel channel, ulong messageId)
        {
            await channel.GetMessageAsync(messageId)
                         .Result.DeleteAsync();
        }

        /// <summary>
        ///     Gets the last very last <see cref="DiscordMessage" />.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <returns></returns>
        public static async Task<DiscordMessage> GetLastMessageAsync(this DiscordChannel channel)
        {
            return channel.GetMessagesAsync(1)
                          .Result.First();
        }
    }
}