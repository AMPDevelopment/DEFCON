using Newtonsoft.Json;

namespace Defcon.Core.Entities.Discord.Embeds
{
    public class EmbedFooter
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}