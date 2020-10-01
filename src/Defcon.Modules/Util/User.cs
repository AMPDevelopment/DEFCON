using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Defcon.Data.Users;
using Defcon.Core.Entities.Discord.Embeds;
using Defcon.Library.Attributes;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Util
{
    [Category("Tools & Utilities")]
    [Group("User")]
    [Aliases("WhoIs")]
    [Description("Shows you details about a user on the server.")]
    [RequireGuild]
    public class User : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public User(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task WhoIs(CommandContext context, [Description("The suspect.")] DiscordUser user = null)
        {
            if (user == null) user = context.User;
            await WhoIsPreset(context, user.Id);
        }

        private async Task WhoIsPreset(CommandContext context, ulong userId)
        {
            await redis.InitUser(userId);
            var member = await context.Guild.GetMemberAsync(userId);
            var nickname = string.IsNullOrWhiteSpace(member.Nickname) ? string.Empty : $"({member.Nickname})";
            var owners = context.Client.CurrentApplication.Owners;

            var author = new EmbedAuthor { Name = $"{member.GetUsertag()} {nickname}", Icon = member.AvatarUrl };

            var description = new StringBuilder();

            if (member.IsOwner)
            {
                description.AppendLine(owners.Any(x => x.Id == member.Id)
                                           ? $"{DiscordEmoji.FromGuildEmote(context.Client, EmojiLibrary.Verified)} {Formatter.InlineCode("Guild and Bot Owner")}"
                                           : $"{DiscordEmoji.FromGuildEmote(context.Client, EmojiLibrary.Verified)} {Formatter.InlineCode("Guild Owner")}");
            }
            else if (owners.Any(x => x.Id == member.Id))
            {
                description.AppendLine($"{DiscordEmoji.FromGuildEmote(context.Client, EmojiLibrary.Verified)} {Formatter.InlineCode("Bot Owner")}");
            }
            else if (member.IsBot)
            {
                description.AppendLine(Formatter.InlineCode("`[BOT]`"));
            }

            var userDays = await member.GetDaysExisting();
            var userSinceDays = userDays == 1 ? $"yesterday" : userDays == 0 ? "today" : $"{Formatter.Bold($"{userDays}")} days";

            var memberDays = await member.GetMemberDays();
            var memberSinceDays = memberDays == 1 ? $"yesterday" : memberDays == 0 ? "today" : $"{Formatter.Bold($"{memberDays}")} days ago";

            description.AppendLine($"Identity: `{member.Id}`")
                       .AppendLine($"Registered: {await member.CreatedAtLongDateTimeString()} ({userSinceDays})")
                       .AppendLine($"Joined: {await member.JoinedAtLongDateTimeString()} ({memberSinceDays})")
                       .AppendLine($"Join Position: #{await JoinPosition(member, context.Guild)}");


            var roles = string.Empty;

            if (member.Roles.Any())
            {
                var rolesOrdered = member.Roles.ToList().OrderByDescending(x => x.Position);
                roles = rolesOrdered.Aggregate(roles, (current, role) => current + $"<@&{role.Id}> ");
            }
            else
            {
                roles = "None";
            }

            var userInfractions = redis.GetAsync<Defcon.Data.Users.User>(RedisKeyNaming.User(userId)).GetAwaiter().GetResult().Infractions;

            var userGuildInfractions = userInfractions.Where(x => x.GuildId == context.Guild.Id)
                                                      .ToList();

            var infractions = new StringBuilder().AppendLine($"Warnings: {userGuildInfractions.Count(x => x.InfractionType == InfractionType.Warning)}")
                                                 .AppendLine($"Mutes: {userGuildInfractions.Count(x => x.InfractionType == InfractionType.Mute)}")
                                                 .AppendLine($"Kicks: {userGuildInfractions.Count(x => x.InfractionType == InfractionType.Kick)}").ToString();

            var keys = await UserKeyPermissions(member);

            var fields = new List<EmbedField>
            {
                new EmbedField {Inline = true, Name = "Boosting since", Value = await member.PremiumSinceLongDateTimeString()},
                new EmbedField {Inline = true, Name = "Server Infractions", Value = infractions},
                new EmbedField {Inline = false, Name = "Roles", Value = roles}
            };

            if (!string.IsNullOrWhiteSpace(keys[0]))
            {
                fields.Add(new EmbedField()
                {
                    Name = "Key Permissions",
                    Value = keys[0],
                    Inline = false
                });
            }

            if (!string.IsNullOrWhiteSpace(keys[1]))
            {
                fields.Add(new EmbedField()
                {
                    Name = "Acknowledgement",
                    Value = keys[1],
                    Inline = false
                });
            }
            
            var embed = new Embed
            {
                Description = description.ToString(),
                Thumbnail = member.AvatarUrl,
                Author = author,
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed);
        }

        private async Task<int> JoinPosition(DiscordMember member, DiscordGuild guild)
        {
            var members = await guild.GetAllMembersAsync();

            var joinPosition = members.OrderBy(x => x.JoinedAt).ToList().FindIndex(x => x.Id == member.Id) + 1;

            return joinPosition;
        }

        private static async Task<string[]> UserKeyPermissions(DiscordMember member)
        {
            var keys = new []{"", ""};
            var acknowledgement = string.Empty;
            var permissions = new List<string>();

            if (member.IsOwner)
            {
                acknowledgement = "Server Owner";
            }
            else if (member.Roles.Any(x => x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed))
            {
                permissions.Add(Permissions.Administrator.ToPermissionString());
                permissions.Add(Permissions.ManageGuild.ToPermissionString());
                permissions.Add(Permissions.BanMembers.ToPermissionString());
                permissions.Add(Permissions.KickMembers.ToPermissionString());
                permissions.Add(Permissions.ManageChannels.ToPermissionString());
                permissions.Add(Permissions.ManageWebhooks.ToPermissionString());
                permissions.Add(Permissions.ManageRoles.ToPermissionString());
                permissions.Add(Permissions.ManageMessages.ToPermissionString());
                permissions.Add(Permissions.ManageEmojis.ToPermissionString());
                permissions.Add(Permissions.ManageNicknames.ToPermissionString());

                acknowledgement = "Server Administration";
            }
            else
            {
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

                if (member.Roles.Any(x => x.CheckPermission(Permissions.ManageGuild) == PermissionLevel.Allowed))
                {
                    acknowledgement = "Server Manager";
                }
                else if (member.Roles.Any(x => x.CheckPermission(Permissions.BanMembers) == PermissionLevel.Allowed ||
                         x.CheckPermission(Permissions.KickMembers) == PermissionLevel.Allowed ||
                         x.CheckPermission(Permissions.ManageChannels) == PermissionLevel.Allowed ||
                         x.CheckPermission(Permissions.ManageWebhooks) == PermissionLevel.Allowed ||
                         x.CheckPermission(Permissions.ManageRoles) == PermissionLevel.Allowed ||
                         x.CheckPermission(Permissions.ManageMessages) == PermissionLevel.Allowed ||
                         x.CheckPermission(Permissions.ManageEmojis) == PermissionLevel.Allowed ||
                         x.CheckPermission(Permissions.ManageNicknames) == PermissionLevel.Allowed))
                {
                    acknowledgement = "Server Moderator";
                }
            }

            keys[0] = string.Join(", ", permissions);
            keys[1] = acknowledgement;

            return keys;
        }
    }
}