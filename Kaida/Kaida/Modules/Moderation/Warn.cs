using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Data.Users;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Extensions;
using Kaida.Library.Redis;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Moderation
{
    [Group("Warn")]
    [RequireGuild]
    public class Warn : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Warn(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task WarnMember(CommandContext context, DiscordMember member, [RemainingText] string reason = "No reason given.")
        {
            if (await redis.IsModerator(context.Guild.Id, context.Member) == false)
            {
                await context.RespondAsync("You are not a moderator!");
            }
            else if (context.Member == member)
            {
                await context.RespondAsync("You can not warn yourself!");
            }
            else
            {
                var userData = await redis.InitUser(member.Id);

                userData.Infractions.Add(new Infraction()
                {
                    Id = ++userData.InfractionId,
                    ModeratorId = context.User.Id,
                    ModeratorUsername = context.User.GetUsertag(),
                    GuildId = context.Guild.Id,
                    InfractionType = InfractionType.Warning,
                    Reason = reason,
                    Date = DateTimeOffset.UtcNow
                });

                await redis.ReplaceAsync<User>(RedisKeyNaming.User(member.Id), userData);

                var description = new StringBuilder().AppendLine($"Moderator: {context.User.GetUsertag()} {Formatter.InlineCode($"{context.User.Id}")}")
                                                     .AppendLine($"Reason: {reason}").ToString();

                var embed = new Embed()
                {
                    Title = $"{member.Username} has been warned!",
                    Description = description,
                    Footer = new EmbedFooter()
                    {
                        Text = $"Infractions: {userData.Infractions.Count}"
                    }
                };

                await context.SendEmbedMessageAsync(embed);

                embed.Title = $"You have been warned on {context.Guild.Name}";
                await member.SendEmbedMessageAsync(embed);
            }
        }
    }
}