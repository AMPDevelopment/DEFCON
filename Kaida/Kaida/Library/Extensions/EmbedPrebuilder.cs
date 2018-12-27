using DSharpPlus.Entities;

namespace Kaida.Library.Extensions
{
    public static class EmbedPrebuilder
    {
        public static DiscordEmbedBuilder.EmbedFooter SeslFooter()
        {
            return new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Configuration.Web,
                IconUrl = Configuration.Logo
            };
        }
    }
}
