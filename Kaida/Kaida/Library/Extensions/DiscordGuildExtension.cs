using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Kaida.Library.Extensions
{
    public static class DiscordGuildExtension
    {
        /// <summary>
        ///     Gets a list of <see cref="DiscordChannel" /> of the type <see cref="ChannelType.Text" />.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild" />.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetTextChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.Text)
                        .ToList();
        }

        /// <summary>
        ///     Gets a list of <see cref="DiscordChannel" /> of the type <see cref="ChannelType.Voice" />.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild" />.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetVoiceChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.Voice)
                        .ToList();
        }

        /// <summary>
        ///     Gets a list of <see cref="DiscordChannel" /> of the type <see cref="ChannelType.Category" />.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild" />.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetCategoryChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.Category)
                        .ToList();
        }

        /// <summary>
        ///     Gets a list of <see cref="DiscordChannel" /> of the type <see cref="ChannelType.News" />.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild" />.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetNewsChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.News)
                        .ToList();
        }

        /// <summary>
        ///     Gets a list of <see cref="DiscordRole" /> from the <see cref="DiscordGuild" />.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild" />.</param>
        /// <returns>Returns a list of <see cref="DiscordRole" />.</returns>
        public static async Task<List<DiscordRole>> GetRoles(this DiscordGuild guild)
        {
            return guild.Roles.Values.Where(x => x.Name != "@everyone")
                        .ToList();
        }

        /// <summary>
        ///     Gets a list of <see cref="DiscordRole" /> which are below the <see cref="DiscordClient" /> from the
        ///     <see cref="DiscordGuild" />.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild" />.</param>
        /// <returns>Returns a list of <see cref="DiscordRole" />.</returns>
        public static async Task<List<DiscordRole>> GetRolesBelowBot(this DiscordGuild guild)
        {
            var botRoles = guild.GetMemberAsync(guild.CurrentMember.Id)
                                .Result.Roles.OrderByDescending(x => x.Position);
            var botRole = botRoles.FirstOrDefault(x => x.Name != "@everyone" && x.IsManaged);

            return guild.Roles.Values.Where(x => botRole != null && x.Position < botRole.Position)
                        .ToList();
        }

        public static async Task<string> CreatedAtLongDateTimeString(this DiscordGuild guild)
        {
            return $"{guild.CreationTimestamp.UtcDateTime.ToLongDateString()}, {guild.CreationTimestamp.UtcDateTime.ToShortTimeString()}";
        }

        public static async Task<string> GetPremiumTier(this DiscordGuild guild)
        {
            var premiumTier = string.Empty;

            switch (guild.PremiumTier)
            {
                case PremiumTier.None:
                    premiumTier = "Level 0";

                    break;
                case PremiumTier.Tier_1:
                    premiumTier = "Level 1";

                    break;
                case PremiumTier.Tier_2:
                    premiumTier = "Level 2";

                    break;
                case PremiumTier.Tier_3:
                    premiumTier = "Level 3";

                    break;
                case PremiumTier.Unknown:
                    premiumTier = "Unknown Level";

                    break;
            }

            return premiumTier;
        }
    }
}