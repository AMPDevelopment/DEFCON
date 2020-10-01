using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Defcon.Core.Entities.Discord.Embeds;
using Defcon.Library.Attributes;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Util
{
    [Category("Tools & Utilities")]
    [Group("Server")]
    [RequireGuild]
    public class Guild : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Guild(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [Command("Info")]
        [Aliases("I")]
        [Description("Shows the server information.")]
        public async Task Info(CommandContext context)
        {
            var guild = context.Guild;
            var channels = guild.GetChannelsAsync().ConfigureAwait(true).GetAwaiter().GetResult().ToList();
            var roles = await guild.GetRoles();
            var emojis = guild.GetEmojisAsync().ConfigureAwait(true).GetAwaiter().GetResult().ToList();
            var members = guild.GetAllMembersAsync().ConfigureAwait(true).GetAwaiter().GetResult().ToList();

            var owner = guild.Owner;
            var prefix = redis.GetAsync<Defcon.Data.Guilds.Guild>(RedisKeyNaming.Guild(context.Guild.Id)).GetAwaiter().GetResult().Prefix;

            var guildAuthor = new EmbedAuthor { Name = guild.Name, Icon = guild.IconUrl };

            var ownerDetails = new StringBuilder()
                              .AppendLine($"Username: {owner.Mention} {Formatter.InlineCode(owner.GetUsertag())}")
                              .AppendLine($"Identity: {Formatter.InlineCode(owner.Id.ToString())}")
                              .ToString();

            var premiumTierCount = guild.PremiumSubscriptionCount;
            var premiumTierSubsLabel = premiumTierCount == 1 ? "1 subscription" : $"{premiumTierCount} subscriptions";
            var premiumTierDetails = new StringBuilder()
                                    .AppendLine($"{await guild.GetPremiumTier()}")
                                    .AppendLine(premiumTierSubsLabel)
                                    .ToString();

            var membersTotal = guild.MemberCount;
            var botsCount = members.Count(x => x.IsBot);
            var humansCount = membersTotal - botsCount;
            var membersOnlineCount = await members.Online();
            var membersDnDCount = await members.DoNotDisturb();
            var membersIdleCount = await members.Idle();
            var membersOfflineCount = membersTotal - (membersOnlineCount + membersIdleCount + membersDnDCount);

            var membersLabel = members.Count == 1 ? "1 Member" : $"{membersTotal} Members";

            var memberDetails = new StringBuilder().AppendLineBold(":busts_in_silhouette: Humans", humansCount)
                                                   .AppendLineBold(":robot: Bots", botsCount)
                                                   .AppendLineBold(":green_circle: Online", membersOnlineCount)
                                                   .AppendLineBold(":orange_circle: Idle", membersIdleCount)
                                                   .AppendLineBold(":red_circle: DnD", membersDnDCount)
                                                   .AppendLineBold(":white_circle: Offline", membersOfflineCount)
                                                   .ToString();

            var totalChannelsCount = channels.Count;
            var categoryChannelCount = await channels.Categories();
            var textChannelCount = await channels.Texts();
            var nsfwChannelCount = await channels.NSFW();
            var voiceChannelCount = await channels.Voices();
            
            var channelsLabel = totalChannelsCount == 1 ? "1 Channel" : $"{totalChannelsCount} Channels";

            var channelDetails = new StringBuilder().AppendLineBold(":file_folder: Category", categoryChannelCount)
                                                    .AppendLineBold(":speech_balloon: Text", textChannelCount)
                                                    .AppendLineBold(":underage: NSFW", nsfwChannelCount)
                                                    .AppendLineBold(":loud_sound: Voice", voiceChannelCount)
                                                    .ToString();


            var afkChannel = guild.AfkChannel != null ? guild.AfkChannel.Name : "Not set";
            var afkTimeout = guild.AfkTimeout / 60;

            var miscDetails = new StringBuilder().AppendLineBold("AFK Channel", afkChannel)
                                                 .AppendLineBold("AFK Timeout", $"{afkTimeout}min")
                                                 .AppendLineBold("Roles", roles.Count)
                                                 .AppendLineBold("Emojis", emojis.Count)
                                                 .ToString();

            var fields = new List<EmbedField>
            {
                new EmbedField {Inline = false, Name = "Owner", Value = ownerDetails},
                new EmbedField {Inline = true, Name = "Premium", Value = premiumTierDetails},
                new EmbedField {Inline = true, Name = "Verification Level", Value = $"{guild.VerificationLevel}"},
                new EmbedField {Inline = true, Name = "Region", Value = $"{guild.VoiceRegion.Name}"},
                new EmbedField {Inline = true, Name = membersLabel, Value = memberDetails},
                new EmbedField {Inline = true, Name = channelsLabel, Value = channelDetails},
                new EmbedField {Inline = true, Name = "Misc", Value = miscDetails}
            };

            var guildDays = await guild.GetDays();
            var guildSinceDays = guildDays == 1 ? $"yesterday" : guildDays == 0 ? "today" : $"{Formatter.Bold($"{guildDays}")} days ago";

            var embed = new Embed
            {
                Description = new StringBuilder()
                             .AppendLine($"Identity: {Formatter.InlineCode($"{guild.Id}")}")
                             .AppendLine($"Created at: {await guild.CreatedAtLongDateTimeString()} ({guildSinceDays})")
                             .AppendLine($"Server Prefix: {Formatter.InlineCode(prefix)}")
                             .ToString(),
                Thumbnail = guild.IconUrl,
                Author = guildAuthor,
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed);
        }
    }
}