using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Kaida.Library.Extensions
{
    public static class DiscordMemberExtension
    {
        public static async Task<string> JoinedAtLongDateTimeString(this DiscordMember member)
        {
            return $"{member.JoinedAt.UtcDateTime.ToLongDateString()}, {member.JoinedAt.UtcDateTime.ToShortTimeString()}";
        }

        public static async Task<string> PremiumSinceLongDateTimeString(this DiscordMember member)
        {
            var boostingDateTime = member.PremiumSince.GetValueOrDefault().UtcDateTime;

            return boostingDateTime >= member.Guild.CreationTimestamp.UtcDateTime ? $"{boostingDateTime.ToLongDateString()}, {boostingDateTime.ToShortTimeString()}" : "Not boosting";
        }
    }
}