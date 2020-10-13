using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Defcon.Library.Extensions
{
    public static class DiscordChannelExtension
    {
        /// <summary>
        ///     Deletes a list of <see cref="DiscordMessage" /> in the <see cref="DiscordChannel" />.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <param name="messages">Represents a list of <see cref="DiscordMessage" /> which will deleted.</param>
        /// <param name="reason">Represents the reason why those <see cref="DiscordMessage" />s has been deleted.</param>
        /// <returns></returns>
        public static async Task BulkMessagesAsync(this DiscordChannel channel, IEnumerable<DiscordMessage> messages, string reason = null)
        {
            await channel.DeleteMessagesAsync(messages, reason);
        }

        /// <summary>
        ///     Deletes a list of <see cref="DiscordMessage" /> from the <see cref="DiscordMember" /> in the
        ///     <see cref="DiscordChannel" />.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <param name="messages">Represents a list of <see cref="DiscordMessage" /> which will deleted.</param>
        /// <param name="member">
        ///     Represents the suspect <see cref="DiscordMember" /> which will all his
        ///     <see cref="DiscordMessage" />s
        /// </param>
        /// <param name="reason">Represents the reason why those <see cref="DiscordMessage" />s has been deleted.</param>
        /// <returns></returns>
        public static async Task BulkMessagesFromUserAsync(this DiscordChannel channel, IEnumerable<DiscordMessage> messages, DiscordMember member, string reason = null)
        {
            var memberMessage = messages.Where(x => x.Author == member)
                                        .ToList();
            await channel.DeleteMessagesAsync(memberMessage, reason);
        }

        /// <summary>
        ///     Deletes a single <see cref="DiscordMessage" /> by id.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <param name="messageId">Represents the id of the <see cref="DiscordMessage" />.</param>
        /// <returns></returns>
        public static async Task DeleteMessageByIdAsync(this DiscordChannel channel, ulong messageId)
        {
            await channel.GetMessageAsync(messageId)
                         .Result.DeleteAsync();
        }

        /// <summary>
        ///     Gets the last very last <see cref="DiscordMessage" />.
        /// </summary>
        /// <param name="channel">Represents the <see cref="DiscordChannel" />.</param>
        /// <returns></returns>
        public static async Task<DiscordMessage> GetLastMessageAsync(this DiscordChannel channel)
        {
            return channel.GetMessagesAsync(1)
                          .Result.First();
        }

        public static async Task<int> Categories(this List<DiscordChannel> channels)
        {
            return channels.Count(x => x.IsCategory);
        }

        public static async Task<int> NSFW(this List<DiscordChannel> channels)
        {
            return channels.Count(x => x.IsNSFW);
        }

        public static async Task<int> Texts(this List<DiscordChannel> channels)
        {
            return channels.Count(x => x.Type == ChannelType.Text);
        }

        public static async Task<int> Voices(this List<DiscordChannel> channels)
        {
            return channels.Count(x => x.Type == ChannelType.Voice);
        }

        public static string UserLimitToString(this DiscordChannel channel)
        {
            return channel.UserLimit == 0 ? "unlimited" : $"{channel.UserLimit}";
        }
        
        public static string PerUserRateLimitToString(this DiscordChannel channel)
        {
            var perUserRateLimit = string.Empty;
            switch (channel.PerUserRateLimit)
            {
                case 0:
                case null:
                    perUserRateLimit = "unrestricted";
                    break;
                case 5:
                case 10:
                case 15:
                    perUserRateLimit = $"{channel.PerUserRateLimit}s";
                    break;
                case 60:
                case 120:
                case 300:
                case 600:
                case 900:
                case 1800:
                    perUserRateLimit = $"{TimeSpan.FromSeconds((double) channel.PerUserRateLimit).Minutes}m";
                    break;
                case 3600:
                case 7200:
                case 21600:
                    perUserRateLimit = $"{TimeSpan.FromSeconds((double) channel.PerUserRateLimit).Hours}h";
                    break;
            }

            return perUserRateLimit;
        }
    }
}