using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Defcon.Data.Guilds;
using Defcon.Data.Users;
using Defcon.Core.Entities.Discord.Embeds;
using Defcon.Library.Converters;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Library.Services.Infractions
{
    public class InfractionService : IInfractionService
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public InfractionService(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        public async Task CreateInfraction(DiscordGuild guild, DiscordChannel channel, DiscordClient client, DiscordMember moderator, DiscordMember suspect, string reason, InfractionType infractionType)
        {
            var owners = client.CurrentApplication.Owners;
            var verb = infractionType.ToInfractionString().ToLowerInvariant();
            var action = infractionType.ToActionString().ToLowerInvariant();
            var isAdministrator = false;
            var isSuspectAdministrator = false;
            
            foreach (var role in moderator.Roles)
            {
                if (isAdministrator) break;
                if (role.Permissions.HasPermission(Permissions.Administrator))
                {
                    isAdministrator = true;
                }
            }
            
            foreach (var role in suspect.Roles)
            {
                if (isSuspectAdministrator) break;
                if (role.Permissions.HasPermission(Permissions.Administrator))
                {
                    isSuspectAdministrator = true;
                }
            }

            if (owners.Any(x => x.Id == suspect.Id))
            {
                await channel.SendMessageAsync($"You can not {verb} my master!");
            }
            else if (await redis.IsModerator(guild.Id, moderator) == false || !isAdministrator)
            {
                await channel.SendMessageAsync("You are not a moderator or administrator!");
            }
            else if (moderator == suspect)
            {
                await channel.SendMessageAsync($"You can not {verb} yourself!");
            }
            else if (suspect.IsOwner)
            {
                await channel.SendMessageAsync($"You can not {verb} the owner!");
            }
            else if (isSuspectAdministrator)
            {
                await channel.SendMessageAsync($"You can not {verb} a administrator!");
            }
            else if (await redis.IsModerator(guild.Id, suspect))
            {
                var guildData = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guild.Id));

                switch (infractionType)
                {
                    case InfractionType.Ban:
                    case InfractionType.Kick:
                        await channel.SendMessageAsync($"You can not {verb} a moderator!");
                        break;
                    case InfractionType.Warning:
                    {
                        if (!guildData.AllowWarnModerators)
                        {
                            await channel.SendMessageAsync($"You can not {verb} a moderator! [Disabled]");
                        }

                        break;
                    }
                    case InfractionType.Mute:
                    {
                        if (!guildData.AllowMuteModerators)
                        {
                            await channel.SendMessageAsync($"You can not {verb} a moderator! [Disabled]");
                        }

                        break;
                    }
                }
            }
            else
            {
                var userData = await redis.InitUser(suspect.Id);

                userData.Infractions.Add(new Infraction()
                {
                    Id = ++userData.InfractionId,
                    ModeratorId = moderator.Id,
                    ModeratorUsername = moderator.GetUsertag(),
                    GuildId = guild.Id,
                    InfractionType = infractionType,
                    Reason = reason,
                    Date = DateTimeOffset.UtcNow
                });

                await redis.ReplaceAsync<User>(RedisKeyNaming.User(suspect.Id), userData);

                var description = new StringBuilder().AppendLine($"Moderator: {moderator.GetUsertag()} {Formatter.InlineCode($"{moderator.Id}")}")
                                                     .AppendLine($"Reason: {reason}").ToString();

                var embed = new Embed()
                {
                    Title = $"{suspect.Username} has been {action}!",
                    Description = description,
                    Footer = new EmbedFooter()
                    {
                        Text = $"Infractions: {userData.Infractions.Count}"
                    }
                };

                await channel.SendEmbedMessageAsync(embed);

                embed.Title = $"You have been {action} on {guild.Name}";
                await suspect.SendEmbedMessageAsync(embed);

                switch (infractionType)
                {
                    case InfractionType.Kick:
                        await suspect.RemoveAsync(reason);
                        break;
                    case InfractionType.Ban:
                        await guild.BanMemberAsync(suspect, 7, reason);
                        break;
                }
            }
        }

        public async Task ManageModerators(DiscordGuild guild, DiscordChannel channel, DiscordRole role, bool isAdd)
        {
            var guildData = await redis.InitGuild(guild.Id);
            var roleName = role.Name;

            if (guildData.ModeratorRoleIds.Contains(role.Id))
            {
                guildData.ModeratorRoleIds.Remove(role.Id);
                await channel.SendMessageAsync($"Removed `{roleName}` role from the moderation team of this guild.");
            }
            else
            {
                guildData.ModeratorRoleIds.Add(role.Id);
                await channel.SendMessageAsync($"Added `{roleName}` role to the moderation team of this guild.");
            }

            await redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(guild.Id), guildData);
        }

        public async Task ViewInfractions(DiscordGuild guild, DiscordChannel channel, DiscordMember suspect)
        {
            var userData = await redis.InitUser(suspect.Id);
            var infractions = userData.Infractions;
            if (infractions.Count > 0)
            {
                if (infractions.Any(x => x.GuildId == guild.Id))
                {
                    var fields = (from infraction in userData.Infractions.Where(x => x.GuildId == guild.Id)
                                                             .OrderByDescending(x => x.Date)
                                  let infractionDetails = new StringBuilder().AppendLine($"{Formatter.Bold("Moderator:")} {infraction.ModeratorUsername} {Formatter.InlineCode($"{infraction.ModeratorId}")}")
                                                                             .AppendLine($"{Formatter.Bold("Type:")} {infraction.InfractionType.ToInfractionString()}")
                                                                             .AppendLine($"{Formatter.Bold("Reason:")} {infraction.Reason}")
                                                                             .AppendLine($"{Formatter.Bold("Date:")} {infraction.Date.DateTime}")
                                                                             .ToString()
                                  select new EmbedField {Inline = false, Name = $"Case #{infraction.Id}", Value = infractionDetails}).ToList();

                    var embed = new Embed()
                    {
                        Title = "Infraction list",
                        Thumbnail = suspect.AvatarUrl,
                        Description = $"{Formatter.Bold("Suspect:")} {suspect.GetUsertag()} {Formatter.InlineCode($"{suspect.Id}")}\n{Formatter.Bold($"Total infractions:")} {userData.Infractions.Count}",
                        Fields = fields
                    };

                    await channel.SendEmbedMessageAsync(embed);
                }
                else
                {
                    var infractionsOutsideGuild = infractions.Count(x => x.GuildId != guild.Id);
                    await channel.SendMessageAsync($"{suspect.Username} hasn't committed any crime so far on {guild.Name}, however {suspect.Username} has committed {infractionsOutsideGuild} crime(s) on other servers!");
                }
            }
            else
            {
                await channel.SendMessageAsync($"{suspect.Username} hasn't committed any crime so far on {guild.Name}");
            }
        }
    }
}
