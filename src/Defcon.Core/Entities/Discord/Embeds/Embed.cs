using System.Collections.Generic;
using DSharpPlus.Entities;

namespace Defcon.Core.Entities.Discord.Embeds
{
    public class Embed
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DiscordColor Color { get; set; } = DiscordColor.None;
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public EmbedAuthor Author { get; set; }
        public List<EmbedField> Fields { get; set; }
        public EmbedFooter Footer { get; set; }
    }
}