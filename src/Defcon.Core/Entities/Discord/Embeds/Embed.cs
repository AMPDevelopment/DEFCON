using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Defcon.Core.Entities.Discord.Embeds
{
    public class Embed
    {
        [JsonProperty("color")]
        private string color;

        private DiscordColor? embedColor;
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }

        public DiscordColor Color
        {
            get
            {
                if (embedColor.HasValue) return embedColor.GetValueOrDefault();
                
                embedColor = string.IsNullOrWhiteSpace(color) ? DiscordColor.None : new DiscordColor(color);
                return embedColor.GetValueOrDefault();

            }
            set => embedColor = value;
        }

        [JsonProperty("image")]
        public string Image { get; set; }
        
        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
        
        [JsonProperty("author")]
        public EmbedAuthor Author { get; set; }
        
        [JsonProperty("fields")]
        public List<EmbedField> Fields { get; set; }
        
        [JsonProperty("footer")]
        public EmbedFooter Footer { get; set; }

        private DiscordColor ConvertHex(string color)
        {
            return new DiscordColor(color);
        }
    }
}