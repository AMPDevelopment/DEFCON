using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Kaida.Entities.Discord;

namespace Kaida.Library.Extensions
{
    public static class EmbedMessage
    {
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
        public static async Task EmbeddedMessage(this CommandContext context, 
            string title = null,
            string description = null,
            DiscordColor? color = null, 
            DiscordEmbedAuthor author = null, 
            List<EmbedField> fields = null,
            string url = null,
            string thumbnailUrl = null, 
            string image = null, 
            DiscordEmbedFooter footer = null,
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
                embeddedMessage.WithAuthor(author.Name, author.Url.ToString(), author.IconUrl.ToString());
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
                embeddedMessage.WithFooter(footer.Text, footer.IconUrl.ToString());
            }
            else
            {
                embeddedMessage.WithFooter();
            }

            if (timestamp != null)
            {
                embeddedMessage.Timestamp = timestamp;
            }

            await context.Channel.SendMessageAsync("", false, embeddedMessage);
        }

        public static async Task EmbeddedFilteredMessage(this CommandContext context, string content)
        {
            var filteredParameters = FilteredParameters(content);
            var title = filteredParameters[0] as string;
            var description = filteredParameters[1] as string;
            var thumbnail = filteredParameters[2] as string;
            var image = filteredParameters[3] as string;
            var url = filteredParameters[4] as string;
            var fields = new List<EmbedField>();

            if (filteredParameters[5] is List<EmbedField> filteredfields)
            {
                fields = filteredfields;
            }

            await context.EmbeddedMessage(title, description, fields: fields, thumbnailUrl: thumbnail, image: image, url: url);
        }

        public static async Task EmbeddedFilteredMessage(this DiscordMessage message, string content)
        {
            var filteredParameters = FilteredParameters(content);
            var embed = new DiscordEmbedBuilder
            {
                Title = filteredParameters[0] as string,
                Description = filteredParameters[1] as string,
                ThumbnailUrl = filteredParameters[2] as string,
                ImageUrl = filteredParameters[3] as string,
                Url = filteredParameters[4] as string,
                Color = Color
            };

            if (filteredParameters[5] is List<EmbedField> fields)
            {
                foreach (var field in fields)
                {
                    embed.AddField(field.Name, field.Value, field.Inline);
                }
            }

            await message.ModifyAsync("", embed.Build());
        }

        private static List<object> FilteredParameters(string content)
        {
            var title = string.Empty;
            var description = string.Empty;
            var thumbnail = string.Empty;
            var image = string.Empty;
            var url = string.Empty;
            var fieldName = string.Empty;
            var fieldValue = string.Empty;
            var fields = new List<EmbedField>();
            foreach (var contentString in content.Split("|"))
            {
                var filteredTitle = FilterArgs(contentString, "title=");
                title = filteredTitle == string.Empty ? title : filteredTitle;
                var filteredDescription = FilterArgs(contentString, "description=");
                description = filteredDescription == string.Empty ? description : filteredDescription;
                var filteredThumbnail = FilterArgs(contentString, "thumbnail=");
                thumbnail = filteredThumbnail == string.Empty ? thumbnail : filteredThumbnail;
                var filteredImage = FilterArgs(contentString, "image=");
                image = filteredImage == string.Empty ? image : filteredImage;
                var filteredUrl = FilterArgs(contentString, "url=");
                url = filteredUrl == string.Empty ? url : filteredUrl;
                var filteredFieldName = FilterArgs(contentString, "name=");
                fieldName = filteredFieldName == string.Empty ? fieldName : filteredFieldName;
                var filteredFieldValue = FilterArgs(contentString, "value=");
                fieldValue = filteredFieldValue == string.Empty ? fieldValue : filteredFieldValue;

                if (fieldName != string.Empty && fieldValue != string.Empty)
                {
                    fields.Add(new EmbedField
                    {
                        Name = fieldName,
                        Value = fieldValue,
                        Inline = true
                    });

                    fieldName = string.Empty;
                    fieldValue = string.Empty;
                }
            }

            return new List<object>
            {
                title,
                description,
                thumbnail,
                image,
                url,
                fields
            };
        }

        private static string FilterArgs(string content, string arg)
        {
            var filteredContent = string.Empty;
            if (content.ToLowerInvariant().Contains(arg))
            {
                filteredContent = content.TrimStart('=').TrimEnd('|');
                filteredContent = filteredContent.Substring(filteredContent.IndexOf('=') + 1);
            }

            return filteredContent;
        }
    }
}
