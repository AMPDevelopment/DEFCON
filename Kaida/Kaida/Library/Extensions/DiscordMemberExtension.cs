using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

            if (boostingDateTime>=member.Guild.CreationTimestamp.UtcDateTime)
            {
                return $"{boostingDateTime.ToLongDateString()}, {boostingDateTime.ToShortTimeString()}";
            }
            else
            {
                return "Not boosting";
            }
        }
    }
}
