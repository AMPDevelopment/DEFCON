using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Attributes;
using Kaida.Library.Converters;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Moderation
{
    [Category("Moderation")]
    [Group("Infractions")]
    [RequireGuild]
    public class Infractions : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Infractions(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task View(CommandContext context, DiscordMember member = null)
        {
            member = member == null ? context.Member : member;
            var userData = await redis.InitUser(member.Id);
            var infractions = userData.Infractions;
            if (infractions.Count > 0)
            {
                if (infractions.Any(x => x.GuildId == context.Guild.Id))
                {
                    var fields = (from infraction in userData.Infractions.Where(x => x.GuildId == context.Guild.Id)
                                                             .OrderByDescending(x => x.Date)
                                  let infractionDetails = new StringBuilder().AppendLine($"{Formatter.Bold("Moderator:")} {infraction.ModeratorUsername} {Formatter.InlineCode($"{infraction.ModeratorId}")}")
                                                                             .AppendLine($"{Formatter.Bold("Type:")} {InfractionTypeConverter.ToString(infraction.InfractionType)}")
                                                                             .AppendLine($"{Formatter.Bold("Reason:")} {infraction.Reason}")
                                                                             .AppendLine($"{Formatter.Bold("Date:")} {infraction.Date.DateTime}")
                                                                             .ToString()
                                  select new EmbedField {Inline = false, Name = $"Case #{infraction.Id}", Value = infractionDetails}).ToList();

                    var embed = new Embed()
                    {
                        Title = "Infraction list",
                        ThumbnailUrl = member.AvatarUrl,
                        Description = $"{Formatter.Bold("Suspect:")} {member.GetUsertag()} {Formatter.InlineCode($"{member.Id}")}\n{Formatter.Bold($"Total infractions:")} {userData.Infractions.Count}",
                        Fields = fields
                    };

                    await context.SendEmbedMessageAsync(embed);
                }
                else
                {
                    var infractionsOutsideGuild = infractions.Count(x => x.GuildId != context.Guild.Id);
                    await context.RespondAsync($"{member.Username} hasn't committed any crime so far on {context.Guild.Name}, however {member.Username} has committed {infractionsOutsideGuild} crime(s) on other servers!");
                }
            }
            else
            {
                await context.RespondAsync($"{member.Username} hasn't committed any crime so far on {context.Guild.Name}");
            }
        }
    }
}