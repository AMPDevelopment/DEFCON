using System;
using System.Threading.Tasks;
using Defcon.Core.Entities.Discord.Embeds;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Defcon.Library.Extensions
{
    public static class EmbedBuilderExtension
    {
        /// <summary>
        ///     Advanced embedded message.
        /// </summary>
        /// <param name="context">Discord's socket command context.</param>
        /// <param name="embed"></param>
        /// <param name="embedFooterStyle"></param>
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

        public static async Task SendJsonEmbedMessageAsync(this CommandContext context, string content)
        {
            await context.SendEmbedMessageAsync(JsonEmbedBuilder(content), EmbedFooterStyle.None);
        }

        public static async Task EditJsonEmbedMessageAsync(this DiscordMessage message, string content)
        {
            var embedBuilder = EmbedBuilder(JsonEmbedBuilder(content), message.Author, EmbedFooterStyle.None);

            await message.ModifyAsync("", embedBuilder);
        }

        public static DiscordEmbedBuilder AddRequestedByFooter(this DiscordEmbedBuilder embed, DiscordUser user)
        {
            embed.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Requested by {user.GetUsertag()} | {user.Id}", 
                IconUrl = user.AvatarUrl
            };

            return embed;
        }

        private static DiscordEmbed EmbedBuilder(Embed embed, DiscordUser user, EmbedFooterStyle embedFooterStyle = EmbedFooterStyle.Default)
        {
            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = !string.IsNullOrWhiteSpace(embed.Title) ? embed.Title : null,
                Description = !string.IsNullOrWhiteSpace(embed.Description) ? embed.Description : null,
                Url = !string.IsNullOrWhiteSpace(embed.Url) ? embed.Url : null,
                Color = embed.Color,
                ImageUrl = !string.IsNullOrWhiteSpace(embed.Image) ? embed.Image : null,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = !string.IsNullOrWhiteSpace(embed.Thumbnail) ? embed.Thumbnail : null
                },
                Timestamp = DateTimeOffset.UtcNow
            };

            if (embed.Author != null)
            {
                embedBuilder.WithAuthor(embed.Author.Name, embed.Author.Url, embed.Author.Icon);
            }

            if (embed.Fields != null)
            {
                foreach (var field in embed.Fields)
                {
                    embedBuilder.AddField(field.Name, field.Value, field.Inline);
                }
            }

            if (embed.Footer != null)
            {
                embedBuilder.WithFooter(embed.Footer.Text, embed.Footer.Icon);
            }
            else
            {
                if (embedFooterStyle == EmbedFooterStyle.Default)
                {
                    if (user != null)
                    {
                        embedBuilder.WithFooter($"Requested by {user.GetUsertag()} | {user.Id}", user.AvatarUrl);
                    }
                }
            }
            
            return embedBuilder.Build();
        }

        private static Embed JsonEmbedBuilder(string content)
        {
            return JsonConvert.DeserializeObject<Embed>(content);
        }
    }

    public enum EmbedFooterStyle
    {
        None,
        Default
    }
}