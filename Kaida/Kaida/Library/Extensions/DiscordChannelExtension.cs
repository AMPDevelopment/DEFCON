using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Kaida.Entities.Discord;

namespace Kaida.Library.Extensions
{
    public static class DiscordChannelExtension
    {
        /// <summary>
        /// Deletes a list of <see cref="DiscordMessage"/> in the <see cref="DiscordChannel"/>.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel"/>.</param>
        /// <param name="messages">Represents a list of <see cref="DiscordMessage"/> which will deleted.</param>
        /// <param name="reason">Represents the reason why those <see cref="DiscordMessage"/>s has been deleted.</param>
        /// <returns></returns>
        public static async Task BulkMessagesAsync(this DiscordChannel channel, IEnumerable<DiscordMessage> messages, string reason = null)
        {
            await channel.DeleteMessagesAsync(messages.ToList(), reason);
        }

        /// <summary>
        /// Deletes a list of <see cref="DiscordMessage"/> from the <see cref="DiscordMember"/> in the <see cref="DiscordChannel"/>.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel"/>.</param>
        /// <param name="messages">Represents a list of <see cref="DiscordMessage"/> which will deleted.</param>
        /// <param name="member">Represents the suspect <see cref="DiscordMember"/> which will all his <see cref="DiscordMessage"/>s</param>
        /// <param name="reason">Represents the reason why those <see cref="DiscordMessage"/>s has been deleted.</param>
        /// <returns></returns>
        public static async Task BulkMessagesFromUserAsync(this DiscordChannel channel, IEnumerable<DiscordMessage> messages, DiscordMember member, string reason = null)
        {
            var memberMessage = messages.Where(x => x.Author == member).ToList();
            await channel.DeleteMessagesAsync(memberMessage, reason);
        }

        /// <summary>
        /// Deletes a single <see cref="DiscordMessage"/> by id.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel"/>.</param>
        /// <param name="messageId">Represents the id of the <see cref="DiscordMessage"/>.</param>
        /// <returns></returns>
        public static async Task DeleteMessageByIdAsync(this DiscordChannel channel, ulong messageId)
        {
            await channel.GetMessageAsync(messageId).Result.DeleteAsync();
        }

        /// <summary>
        /// Gets the last very last <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel"/>.</param>
        /// <returns></returns>
        public static async Task<DiscordMessage> GetLastMessageAsync(this DiscordChannel channel)
        {
            return channel.GetMessagesAsync(1).Result.First();
        }

        private static readonly DiscordColor Color = new DiscordColor(245, 139, 54);

        /// <summary>
        ///     Advanced embeded message.
        /// </summary>
        /// <param name="context">Discord's socket command context.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="color">The color.</param>
        /// <param name="author">The author.</param>
        /// <param name="fields">A list of <c>EmbedFieldBuilder</c>'s</param>
        /// <param name="url">The url.</param>
        /// <param name="thumbnailUrl">The thumbnail url.</param>
        /// <param name="image">The image.</param>
        /// <param name="footer">The footer.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>Returns a embeded message in the channel in which the call was triggered.</returns>
        public static async Task SendEmbedMessageAsync(this DiscordChannel channel,
            string title = null,
            string description = null,
            DiscordColor? color = null,
            DiscordEmbedBuilder.EmbedAuthor author = null,
            List<EmbedField> fields = null,
            string url = null,
            string thumbnailUrl = null,
            string image = null,
            DiscordEmbedBuilder.EmbedFooter footer = null,
            DateTimeOffset? timestamp = null)
        {
            var embeddedMessage = new DiscordEmbedBuilder
            {
                Color = color ?? Color
            };

            if (!string.IsNullOrWhiteSpace(title))
            {
                embeddedMessage.Title = title;
            }

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    embeddedMessage.AddField(field.Name, field.Value, field.Inline);
                }
            }

            if (author != null)
            {
                embeddedMessage.Author = author;
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                embeddedMessage.Url = url;
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                embeddedMessage.Description = description;
            }

            if (!string.IsNullOrWhiteSpace(thumbnailUrl))
            {
                embeddedMessage.ThumbnailUrl = thumbnailUrl;
            }

            if (!string.IsNullOrWhiteSpace(image))
            {
                embeddedMessage.ImageUrl = image;
            }

            if (footer != null)
            {
                embeddedMessage.Footer = footer;
            }

            if (timestamp != null)
            {
                embeddedMessage.Timestamp = timestamp;
            }

            await channel.SendMessageAsync("", false, embeddedMessage);
        }
    }
}
