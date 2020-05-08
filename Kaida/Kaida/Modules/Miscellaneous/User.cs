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
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules.Miscellaneous
{
    [Group("UserInfo")]
    [Aliases("WhoIs")]
    public class User : BaseCommandModule
    {
        private readonly ILogger logger;

        public User(ILogger logger)
        {
            this.logger = logger;
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
            var member = await context.Guild.GetMemberAsync(userId);
            var nickname = string.IsNullOrWhiteSpace(member.Nickname) ? string.Empty : $"({member.Nickname})";

            var author = new EmbedAuthor { Name = $"{member.GetUsertag()} {nickname}", IconUrl = member.AvatarUrl };

            var description = new StringBuilder();

            if (member.IsOwner)
            {
                description.AppendLine(Formatter.InlineCode("[Owner]"));
            }
            else if (member.IsBot)
            {
                description.AppendLine(Formatter.InlineCode("[Bot]"));
            }

            description.AppendLine($"Identity: `{member.Id}`")
                       .AppendLine($"Registered: {await member.CreatedAtLongDateTimeString()}");

            var roles = string.Empty;

            if (member.Roles.Any())
            {
                var rolesSorted = member.Roles.ToList()
                                        .OrderByDescending(x => x.Position);

                roles = rolesSorted.Aggregate(roles, (current, role) => current + $"<@&{role.Id}> ");
            }
            else
            {
                roles = "None";
            }

            var permissions = await UserKeyPermissions(member);

            var fields = new List<EmbedField>
            {
                new EmbedField {Inline = true, Name = "Joined Server at", Value = await member.JoinedAtLongDateTimeString()},
                new EmbedField {Inline = true, Name = "Boosting since", Value = await member.PremiumSinceLongDateTimeString()},
                new EmbedField {Inline = true, Name = "Server Infractions", Value = "stfu its not ready now"},
                new EmbedField {Inline = false, Name = "Roles", Value = roles}
            };

            if (!string.IsNullOrWhiteSpace(permissions))
            {
                fields.Add(new EmbedField()
                {
                    Name = "Key Permissions",
                    Value = permissions,
                    Inline = false
                });
            }

            var embed = new Embed
            {
                Description = description.ToString(),
                ThumbnailUrl = member.AvatarUrl,
                Author = author,
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed);
        }

        private async Task<string> UserKeyPermissions(DiscordMember member)
        {
            var permissions = new List<string>();
            if (member.Roles.Any(x => x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.Administrator.ToPermissionString());
            }

            if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageGuild) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.ManageGuild.ToPermissionString());
            }

            if (member.Roles.Any(x => x.CheckPermission(Permissions.BanMembers) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.BanMembers.ToPermissionString());
            }
            if (member.Roles.Any(x => x.CheckPermission(Permissions.KickMembers) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.KickMembers.ToPermissionString());
            }
            if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageChannels) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.ManageChannels.ToPermissionString());
            }
            if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageWebhooks) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.ManageWebhooks.ToPermissionString());
            }
            if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageRoles) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.ManageRoles.ToPermissionString());
            }
            if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageMessages) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.ManageMessages.ToPermissionString());
            }
            if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageEmojis) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.ManageEmojis.ToPermissionString());
            }
            if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageNicknames) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.ManageNicknames.ToPermissionString());
            }

            return string.Join(", ", permissions);
        }
    }
}