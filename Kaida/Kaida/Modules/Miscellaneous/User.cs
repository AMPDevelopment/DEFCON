using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Entities.Discord;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules.Miscellaneous
{
    [Group("UserInfo")]
    [Aliases("WhoIs")]
    public class User : BaseCommandModule
    {
        private readonly ILogger _logger;

        public User(ILogger logger)
        {
            _logger = logger;
        }

        [GroupCommand]
        [Priority(1)]
        public async Task WhoIs(CommandContext context, DiscordUser user = null)
        {
            if (user == null) user = context.User;
            await WhoIsPreset(context, user.Id);
        }

        [GroupCommand]
        [Priority(2)]
        public async Task WhoIs(CommandContext context, ulong userId)
        {
            await WhoIsPreset(context, userId);
        }

        private async Task WhoIsPreset(CommandContext context, ulong userId)
        {
            var user = await context.Guild.GetMemberAsync(userId);
            var nickname = string.IsNullOrWhiteSpace(user.Nickname) ? string.Empty : $"({user.Nickname})";
            var thumbnailUrl = user.AvatarUrl;

            var description = new StringBuilder().AppendLine($"User identity: `{user.Id}`").AppendLine($"Registered: {await user.CreatedAtLongDateTimeString()}");

            if (user.Verified.HasValue) description.AppendLine($"Verified account: {user.Verified.Value}");

            if (user.PremiumType.HasValue)
            {
                switch (user.PremiumType.Value)
                {
                    case PremiumType.Nitro:
                        description.AppendLine("Premium: Nitro");

                        break;
                    case PremiumType.NitroClassic:
                        description.AppendLine("Premium: Nitro Classic");

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = $"{user.GetUsertag()} {nickname}",
                IconUrl = user.AvatarUrl
            };

            var fields = new List<EmbedField>
            {
                new EmbedField
                {
                    Inline = true, Name = "Joined Server at", Value = await user.JoinedAtLongDateTimeString()
                }
            };

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

            if (user.Roles.Any())
            {
                var rolesSorted = user.Roles.ToList().OrderByDescending(x => x.Position);

                roles = rolesSorted.Aggregate(roles, (current, role) => current + $"<@&{role.Id}> ");
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

            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Requested by {context.User.GetUsertag()} | {context.User.Id}"
            };
            await context.EmbeddedMessage(description: description.ToString(), author: author, fields: fields, thumbnailUrl: thumbnailUrl, footer: footer, timestamp: DateTimeOffset.UtcNow);
        }
    }
}