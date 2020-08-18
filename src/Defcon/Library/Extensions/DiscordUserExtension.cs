using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Defcon.Library.Extensions
{
    public static class DiscordUserExtension
    {
        public static string GetUsertag(this DiscordUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }

        public static async Task<string> CreatedAtLongDateTimeString(this DiscordUser user)
        {
            return $"{user.CreationTimestamp.UtcDateTime.ToLongDateString()}, {user.CreationTimestamp.UtcDateTime.ToShortTimeString()}";
        }

        public static async Task<int> GetDaysExisting(this DiscordUser user)
        {
            var span = DateTimeOffset.UtcNow.Subtract(user.CreationTimestamp.UtcDateTime);
            return (int) span.TotalDays;
        }
    }
}