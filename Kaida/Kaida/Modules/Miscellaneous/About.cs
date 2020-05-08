using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaida.Modules.Miscellaneous
{
    [Group("About")]
    public class About : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IDatabase redis;

        public About(ILogger logger, IDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task Info(CommandContext context)
        {
            var description = new StringBuilder().AppendLine($"App Version: 0.0.2-alpha")
                                                 .AppendLine($"Gateway Version: {context.Client.GatewayVersion}")
                                                 .AppendLine($"DSharpPlus Version: {context.Client.VersionString}")
                                                 .AppendLine($"Servers: soon:tm:")
                                                 .AppendLine($"Users: soon:tm:").ToString();

            var socialMedia = new StringBuilder().AppendLine(Formatter.MaskedUrl("GitHub", new Uri("https://github.com/AMPDevelopment/Kaida")))
                                                 .AppendLine(Formatter.MaskedUrl("Twitter", new Uri("https://twitter.com/onlyonecookie_"))).ToString();

            var fields = new List<EmbedField>
            {
                new EmbedField
                {
                    Inline = true, 
                    Name = "Support", 
                    Value = Formatter.MaskedUrl("Discord", new Uri("https://discord.gg/WgUDVAk"))
                },
                new EmbedField
                {
                    Inline = true, 
                    Name = "Social Media", 
                    Value = socialMedia
                },
                new EmbedField
                {
                    Inline = false,
                    Name = "Copyright",
                    Value = $"Antoine {Formatter.MaskedUrl("OnlyOneCookie", new Uri("https://www.onlyonecookie.ch"))} Martins Pacheco"
                }
            };
            var embed = new Embed
            {
                Title = "About", 
                Description = description,
                ThumbnailUrl = context.Client.CurrentUser.AvatarUrl,
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed);
        }
    }
}