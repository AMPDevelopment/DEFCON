using Newtonsoft.Json;

namespace Defcon.Core.Entities.Discord.Embeds
{
    public class EmbedAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}