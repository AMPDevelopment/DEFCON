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
using StackExchange.Redis;

namespace Kaida.Modules.Miscellaneous
{
    [Group("Guild")]
    [Aliases("Server")]
    public class Guild : BaseCommandModule
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;

        public Guild(ILogger logger, IDatabase redis)
        {
            _logger = logger;
            _redis = redis;
        }

        [Command("Info")]
        [Aliases("I")]
        public async Task Info(CommandContext context)
        {
            var guild = context.Guild;
            var channels = guild.Channels.Values.ToList();
            var roles = guild.Roles.Values.ToList();
            var members = guild.Members.Values.ToList();
            var thumbnailUrl = guild.IconUrl;
            var owner = guild.Owner;
            var prefix = _redis.StringGet($"{context.Guild.Id}:CommandPrefix").ToString();
            var description = new StringBuilder().AppendLine($"Identity: {Formatter.InlineCode($"{guild.Id}")}").AppendLine($"Created at: {await guild.CreatedAtLongDateTimeString()}").AppendLine($"Server Prefix: {Formatter.InlineCode(prefix)}").ToString();

            var guildAuthor = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = guild.Name,
                IconUrl = guild.IconUrl
            };

            var ownerDetails = new StringBuilder().AppendLine($"Username: `{owner.GetUsertag()}`").AppendLine($"Identity: `{owner.Id}`").ToString();

            var botsCount = members.Count(x => x.IsBot);
            var humansCount = guild.MemberCount - botsCount;
            var membersOnlineCount = members.Count(member => member != null && member.Presence.Status == UserStatus.Online);

            var memberDetails = new StringBuilder().AppendLine($"{guild.MemberCount} members").AppendLine($"{membersOnlineCount} online").AppendLine($"{humansCount} humans, {botsCount} bots").ToString();

            var totalChannelsCount = channels.Count;
            var categoryChannelCount = channels.Count(x => x.IsCategory);
            var textChannelCount = channels.Count(x => x.Type == ChannelType.Text);
            var voiceChannelCount = channels.Count(x => x.Type == ChannelType.Voice);

            var totalChannelsLabel = totalChannelsCount == 1 ? "1 total channel" : $"{totalChannelsCount} total channels";
            var categoryChannelLabel = categoryChannelCount == 1 ? "1 category" : $"{categoryChannelCount} categories";
            var textChannelLabel = textChannelCount == 1 ? "1 text channel" : $"{textChannelCount} text channels";
            var voiceChannelLabel = voiceChannelCount == 1 ? "1 voice channel" : $"{voiceChannelCount} voice channels";

            var channelDetails = new StringBuilder().AppendLine(totalChannelsLabel).AppendLine(categoryChannelLabel).AppendLine(textChannelLabel).AppendLine(voiceChannelLabel).ToString();

            var rolesCount = roles.Count(x => x.Name != "@everyone");
            var rolesCountLabel = rolesCount == 1 ? "1 role" : $"{rolesCount} roles";

            var rolesDetails = new StringBuilder().AppendLine(rolesCountLabel).AppendLine($"Use {Formatter.InlineCode($"{prefix}server roles")} to see a list with all roles.").ToString();

            var premiumTierCount = guild.PremiumSubscriptionCount;
            var premiumTierSubsLabel = premiumTierCount == 1 ? "1 subscription" : $"{premiumTierCount} subscriptions";
            var premiumTierDetails = new StringBuilder().AppendLine($"{await guild.GetPremiumTier()}").AppendLine(premiumTierSubsLabel).ToString();

            var fields = new List<EmbedField>
            {
                new EmbedField
                {
                    Inline = false,
                    Name = "Owner",
                    Value = ownerDetails
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Premium",
                    Value = premiumTierDetails
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Verification Level",
                    Value = $"{guild.VerificationLevel}"
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Region",
                    Value = $"{guild.VoiceRegion.Name}"
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Members",
                    Value = memberDetails
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Channels",
                    Value = channelDetails
                },
                new EmbedField
                {
                    Inline = false,
                    Name = "Roles",
                    Value = rolesDetails
                }
            };

            var guildFooter = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Requested by {context.User.GetUsertag()} | {context.User.Id}"
            };

            await context.EmbeddedMessage(description: description, author: guildAuthor, fields: fields, thumbnailUrl: thumbnailUrl, footer: guildFooter, timestamp: DateTimeOffset.UtcNow);
        }

        [Command("Roles")]
        [Aliases("R")]
        public async Task Roles(CommandContext context)
        {
            var guild = context.Guild;
            var roles = guild.Roles.Values;
            var thumbnailUrl = guild.IconUrl;

            var guildAuthor = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = guild.Name,
                IconUrl = guild.IconUrl
            };

            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Requested by {context.User.GetUsertag()} | {context.User.Id}"
            };

            var rolesList = roles.Where(x => x.Name != "@everyone").OrderByDescending(r => r.Position).Aggregate("", (current, x) => current + $"<@&{x.Id}>\n");

            await context.EmbeddedMessage(description: rolesList, author: guildAuthor, footer: footer, thumbnailUrl: thumbnailUrl, timestamp: DateTimeOffset.UtcNow);
        }
    }
}