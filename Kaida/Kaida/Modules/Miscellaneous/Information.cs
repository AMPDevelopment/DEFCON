using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kaida.Library.Extensions;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Entities.Discord;
using System.Linq;

namespace Kaida.Modules.Miscellaneous
{
    public class Information : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Information(ILogger logger)
        {
            _logger = logger;
        }

        [Command("Whois")]
        [Priority(1)]
        public async Task Whois(CommandContext context, DiscordUser user = null)
        {
            if (user == null) user = context.User;
            await WhoisPreset(context, user.Id);
        }

        [Command("Whois")]
        [Priority(2)]
        public async Task Whois(CommandContext context, ulong userId)
        {
            await WhoisPreset(context, userId);
        }

        private async Task WhoisPreset(CommandContext context, ulong userId)
        {
            var user = await context.Guild.GetMemberAsync(userId);
            var description = new StringBuilder()
                .AppendLine($"User identity: `{user.Id}`")
                .AppendLine($"Registered: {await user.CreatedAtLongDateTimeString()}");

            if (user.Verified.HasValue) description.AppendLine($"Verified account: {user.Verified.GetValueOrDefault()}");
            if (user.PremiumType.GetValueOrDefault() != null) 
            {
                switch (user.PremiumType.GetValueOrDefault())
                {
                    case PremiumType.Nitro:
                        description.AppendLine($"Premium: Nitro");
                        break;
                    case PremiumType.NitroClassic:
                        description.AppendLine($"Premium: Nitro Classic");
                        break;
                }
            }
            
            var author = new DiscordEmbedBuilder.EmbedAuthor 
            { 
                Name = $"{user.GetUsertag()} ({user.Nickname})",
                IconUrl = user.AvatarUrl
            };

            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Requested by {context.User.GetUsertag()} | {context.User.Id}"
            };

            
            

            var fields = new List<EmbedField>();

            fields.Add(new EmbedField
            {
                Inline = true,
                Name = "Joined Server at",
                Value = await user.JoinedAtLongDateTimeString()
            });

            if (user.PremiumSince.GetValueOrDefault().Offset != null)
            {
                fields.Add(new EmbedField
                {
                    Inline = true,
                    Name = "Boosting since",
                    Value = await user.PremiumSinceLongDateTimeString()
                });
            }

            fields.Add(new EmbedField
            {
                Inline = true,
                Name = "Server Infractions",
                Value = "stfu its not ready now"
            });

            var roles = string.Empty;
            if (user.Roles.Count() > 0)
            {
                var rolesSorted = user.Roles.ToList().OrderByDescending(x => x.Position);

                foreach (var role in rolesSorted)
                {
                    roles += $"<@&{role.Id}> ";
                }
            }
            else
            {
                roles = "None";
            }

            fields.Add(new EmbedField
            {
                Inline = false,
                Name = "Roles",
                Value = roles
            });

            fields.Add(new EmbedField
            {
                Inline = false,
                Name = "Server Permissions",
                Value = $"{user.Guild.Permissions.GetValueOrDefault()}"
            });

            var thumbnailUrl = user.AvatarUrl;
            await context.EmbeddedMessage(description: description.ToString(), author: author, color: DiscordColor.Turquoise, fields: fields, thumbnailUrl: thumbnailUrl, footer: footer, timestamp: DateTimeOffset.UtcNow);
        }
    }
}
