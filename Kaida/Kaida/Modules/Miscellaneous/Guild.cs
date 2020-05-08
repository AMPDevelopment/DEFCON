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
using StackExchange.Redis;

namespace Kaida.Modules.Miscellaneous
{
    [Group("Guild")]
    [Aliases("Server")]
    public class Guild : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IDatabase redis;

        public Guild(ILogger logger, IDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [Command("Info")]
        [Aliases("I")]
        public async Task Info(CommandContext context)
        {
            var guild = context.Guild;
            var channels = guild.Channels.Values.ToList();
            var roles = guild.Roles.Values.ToList();
            var members = guild.GetAllMembersAsync()
                               .Result.ToList();

            var owner = guild.Owner;
            var prefix = redis.StringGet($"{context.Guild.Id}:CommandPrefix")
                              .ToString();

            var guildAuthor = new EmbedAuthor {Name = guild.Name, IconUrl = guild.IconUrl};

            var ownerDetails = new StringBuilder()
                              .AppendLine($"Username: `{owner.GetUsertag()}`")
                              .AppendLine($"Identity: `{owner.Id}`")
                              .ToString();

            var premiumTierCount = guild.PremiumSubscriptionCount;
            var premiumTierSubsLabel = premiumTierCount == 1 ? "1 subscription" : $"{premiumTierCount} subscriptions";
            var premiumTierDetails = new StringBuilder()
                                    .AppendLine($"{await guild.GetPremiumTier()}")
                                    .AppendLine(premiumTierSubsLabel)
                                    .ToString();

            var botsCount = members.Count(x => x.IsBot);
            var humansCount = guild.MemberCount - botsCount;
            var membersOnlineCount = members.Count(x => x.Presence != null && x.Presence.Status == UserStatus.Online);
            var membersDnDCount = members.Count(x => x.Presence != null && x.Presence.Status == UserStatus.DoNotDisturb);
            var membersIdleCount = members.Count(x => x.Presence != null && x.Presence.Status == UserStatus.Idle);
            var membersLabel = members.Count == 1 ? "1 Member" : $"{members.Count} Members";
            var memberDetails = new StringBuilder()
                               .AppendLine($"{membersOnlineCount} online")
                               .AppendLine($"{membersDnDCount} busy")
                               .AppendLine($"{membersIdleCount} idling")
                               .AppendLine($"{humansCount} humans, {botsCount} bots")
                               .ToString();

            var totalChannelsCount = channels.Count;
            var categoryChannelCount = channels.Count(x => x.IsCategory);
            var textChannelCount = channels.Count(x => x.Type == ChannelType.Text);
            var voiceChannelCount = channels.Count(x => x.Type == ChannelType.Voice);

            var channelsLabel = totalChannelsCount == 1 ? "1 Channel" : $"{totalChannelsCount} Channels";
            var categoryChannelLabel = categoryChannelCount == 1 ? "1 category" : $"{categoryChannelCount} categories";
            var textChannelLabel = textChannelCount == 1 ? "1 text channel" : $"{textChannelCount} text channels";
            var voiceChannelLabel = voiceChannelCount == 1 ? "1 voice channel" : $"{voiceChannelCount} voice channels";

            var channelDetails = new StringBuilder()
                                .AppendLine(categoryChannelLabel)
                                .AppendLine(textChannelLabel)
                                .AppendLine(voiceChannelLabel)
                                .ToString();

            var rolesCount = roles.Count(x => x.Name != "@everyone");
            var rolesLabel = rolesCount == 1 ? "1 Role" : $"{rolesCount} Roles";

            var rolesDetails = $"Use {Formatter.InlineCode($"{prefix}server roles")} to see a list with all roles.";

            var fields = new List<EmbedField>
            {
                new EmbedField {Inline = false, Name = "Owner", Value = ownerDetails},
                new EmbedField {Inline = true, Name = "Premium", Value = premiumTierDetails},
                new EmbedField {Inline = true, Name = "Verification Level", Value = $"{guild.VerificationLevel}"},
                new EmbedField {Inline = true, Name = "Region", Value = $"{guild.VoiceRegion.Name}"},
                new EmbedField {Inline = true, Name = membersLabel, Value = memberDetails},
                new EmbedField {Inline = true, Name = channelsLabel, Value = channelDetails},
                new EmbedField {Inline = false, Name = rolesLabel, Value = rolesDetails}
            };

            var embed = new Embed
            {
                Description = new StringBuilder()
                             .AppendLine($"Identity: {Formatter.InlineCode($"{guild.Id}")}")
                             .AppendLine($"Created at: {await guild.CreatedAtLongDateTimeString()}")
                             .AppendLine($"Server Prefix: {Formatter.InlineCode(prefix)}")
                             .ToString(),
                ThumbnailUrl = guild.IconUrl,
                Author = guildAuthor,
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed);
        }

        [Command("Roles")]
        [Aliases("R")]
        public async Task Roles(CommandContext context)
        {
            var guild = context.Guild;
            var roles = guild.Roles.Values;

            var guildAuthor = new EmbedAuthor {Name = guild.Name, IconUrl = guild.IconUrl};

            var rolesList = roles.Where(x => x.Name != "@everyone")
                                 .OrderByDescending(r => r.Position)
                                 .Aggregate("", (current, x) => current + $"<@&{x.Id}>\n");

            var embed = new Embed {Description = rolesList, ThumbnailUrl = guild.IconUrl, Author = guildAuthor};

            await context.SendEmbedMessageAsync(embed);
        }
    }
}