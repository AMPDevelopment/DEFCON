using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Defcon.Entities.Discord.Embeds;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Defcon.Library.Extensions
{
    public static class EmbedBuilderExtension
    {
        /// <summary>
        ///     Advanced embedded message.
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
        /// <returns>Returns a embedded message in the channel in which the call was triggered.</returns>
        public static async Task SendEmbedMessageAsync(this CommandContext context, Embed embed, EmbedFooterStyle embedFooterStyle = EmbedFooterStyle.Default)
        {
            await context.Channel.SendMessageAsync("", false, EmbedBuilder(embed, context.User, embedFooterStyle));
        }

        public static async Task SendEmbedMessageAsync(this DiscordChannel channel, Embed embed, DiscordUser user = null, EmbedFooterStyle embedFooterStyle = EmbedFooterStyle.Default)
        {
            await channel.SendMessageAsync("", false, EmbedBuilder(embed, user, embedFooterStyle));
        }

        public static async Task SendEmbedMessageAsync(this DiscordMember member, Embed embed, EmbedFooterStyle embedFooterStyle = EmbedFooterStyle.Default)
        {
            await member.SendMessageAsync("", false, EmbedBuilder(embed, member, embedFooterStyle));
        }

        public static async Task SendFilteredEmbedMessageAsync(this CommandContext context, string content)
        {
            var filteredParameters = FilteredParameters(content);

            var title = filteredParameters[0] as string;
            var description = filteredParameters[1] as string;
            var url = filteredParameters[2] as string;
            var thumbnail = filteredParameters[3] as string;
            var image = filteredParameters[4] as string;
            var fields = new List<EmbedField>();

            if (filteredParameters[5] is List<EmbedField> filteredFields)
            {
                fields = filteredFields;
            }

            var embed = new Embed
            {
                Title = title,
                Description = description,
                Url = url,
                ThumbnailUrl = thumbnail,
                ImageUrl = image,
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed, EmbedFooterStyle.None);
        }

        public static async Task SendFilteredEmbedMessageAsync(this DiscordMessage message, string content)
        {
            var filteredParameters = FilteredParameters(content);

            var title = filteredParameters[0] as string;
            var description = filteredParameters[1] as string;
            var url = filteredParameters[2] as string;
            var thumbnail = filteredParameters[3] as string;
            var image = filteredParameters[4] as string;
            var footer = filteredParameters[6] as string;
            var fields = new List<EmbedField>();

            if (filteredParameters[5] is List<EmbedField> filteredFields)
            {
                fields = filteredFields;
            }

            var embed = new Embed
            {
                Title = title,
                Description = description,
                Url = url,
                ThumbnailUrl = thumbnail,
                ImageUrl = image,
                Fields = fields,
                Footer = new EmbedFooter() {Text = footer}
            };


            var embedBuilder = EmbedBuilder(embed, message.Author);

            await message.ModifyAsync("", embedBuilder);
        }

        public static DiscordEmbedBuilder AddRequestedByFooter(this DiscordEmbedBuilder embed, DiscordUser user)
        {
            embed.Footer = new DiscordEmbedBuilder.EmbedFooter {Text = $"Requested by {user.GetUsertag()} | {user.Id}", IconUrl = user.AvatarUrl};

            return embed;
        }

        private static DiscordEmbed EmbedBuilder(Embed embed, DiscordUser user, EmbedFooterStyle embedFooterStyle = EmbedFooterStyle.Default)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = !string.IsNullOrWhiteSpace(embed.Title) ? embed.Title : null,
                Description = !string.IsNullOrWhiteSpace(embed.Description) ? embed.Description : null,
                Url = !string.IsNullOrWhiteSpace(embed.Url) ? embed.Url : null,
                Color = embed.Color.Equals(DiscordColor.None) ? DiscordColor.Turquoise : embed.Color,
                ImageUrl = !string.IsNullOrWhiteSpace(embed.ImageUrl) ? embed.ImageUrl : null,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Url = !string.IsNullOrWhiteSpace(embed.ThumbnailUrl) ? embed.ThumbnailUrl : null
                },
                Timestamp = DateTimeOffset.UtcNow
            };

            if (embed.Author != null)
            {
                embedBuilder.WithAuthor(embed.Author.Name, embed.Author.Url, embed.Author.IconUrl);
            }

            if (embed.Fields != null)
            {
                foreach (var field in embed.Fields)
                {
                    embedBuilder.AddField(field.Name, field.Value, field.Inline);
                }
            }

            if (embedFooterStyle != EmbedFooterStyle.None)
            {
                if (embed.Footer != null)
                {
                    embedBuilder.WithFooter(embed.Footer.Text, embed.Footer.IconUrl);
                }
                else
                {
                    if (user != null)
                    {
                        embedBuilder.WithFooter($"Requested by {user.GetUsertag()} | {user.Id}", user.AvatarUrl);
                    }
                }
            }
            
            return embedBuilder.Build();
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
            var footer = string.Empty;
            var fields = new List<EmbedField>();

            foreach (var contentString in content.Split("|"))
            {
                var filteredTitle = FilterArgs(contentString, "title=");
                title = filteredTitle == string.Empty ? title : filteredTitle;
                var filteredDescription = FilterArgs(contentString, "description=");
                description = filteredDescription == string.Empty ? description : filteredDescription;
                var filteredUrl = FilterArgs(contentString, "url=");
                url = filteredUrl == string.Empty ? url : filteredUrl;
                var filteredThumbnail = FilterArgs(contentString, "thumbnail=");
                thumbnail = filteredThumbnail == string.Empty ? thumbnail : filteredThumbnail;
                var filteredImage = FilterArgs(contentString, "image=");
                image = filteredImage == string.Empty ? image : filteredImage;
                var filteredFieldName = FilterArgs(contentString, "name=");
                fieldName = filteredFieldName == string.Empty ? fieldName : filteredFieldName;
                var filteredFieldValue = FilterArgs(contentString, "value=");
                fieldValue = filteredFieldValue == string.Empty ? fieldValue : filteredFieldValue;
                var filteredFieldInline = FilterArgs(contentString, "inline=");
                var fieldInline = filteredFieldInline == "true" ? true : false;
                var filteredFooter = FilterArgs(contentString, "footer=");
                footer = filteredFooter == string.Empty ? footer : filteredFooter;

                if (fieldName != string.Empty && fieldValue != string.Empty)
                {
                    fields.Add(new EmbedField {Name = fieldName, Value = fieldValue, Inline = fieldInline});

                    fieldName = string.Empty;
                    fieldValue = string.Empty;
                }
            }

            return new List<object>
            {
                title,
                description,
                url,
                thumbnail,
                image,
                fields,
                footer
            };
        }

        private static string FilterArgs(string content, string arg)
        {
            var filteredContent = string.Empty;

            if (content.ToLowerInvariant()
                       .Contains(arg))
            {
                filteredContent = content.TrimStart('=')
                                         .TrimEnd('|');
                filteredContent = filteredContent.Substring(filteredContent.IndexOf('=') + 1);
            }

            return filteredContent;
        }
    }

    public enum EmbedFooterStyle
    {
        None,
        Default
    }
}