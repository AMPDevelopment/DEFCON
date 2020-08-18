using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Defcon.Library.Extensions
{
    public static class DiscordMemberExtension
    {
        public static async Task<string> JoinedAtLongDateTimeString(this DiscordMember member)
        {
            return $"{member.JoinedAt.UtcDateTime.ToLongDateString()}, {member.JoinedAt.UtcDateTime.ToShortTimeString()}";
        }

        public static async Task<int> GetMemberDays(this DiscordMember member)
        {
            var span = DateTimeOffset.UtcNow.Subtract(member.JoinedAt.UtcDateTime);
            return (int) span.TotalDays;
        }

        public static async Task<string> PremiumSinceLongDateTimeString(this DiscordMember member)
        {
            var boostingDateTime = member.PremiumSince.GetValueOrDefault()
                                         .UtcDateTime;

            return boostingDateTime >= member.Guild.CreationTimestamp.UtcDateTime ? $"{boostingDateTime.ToLongDateString()}, {boostingDateTime.ToShortTimeString()}" : "Not boosting";
        }

        public static async Task<int> Online(this List<DiscordMember> members)
        {
            return members.Count(x => x.Presence != null && x.Presence.Status == UserStatus.Online);
        }

        public static async Task<int> Idle(this List<DiscordMember> members)
        {
            return members.Count(x => x.Presence != null && x.Presence.Status == UserStatus.Idle);
        }

        public static async Task<int> DoNotDisturb(this List<DiscordMember> members)
        {
            return members.Count(x => x.Presence != null && x.Presence.Status == UserStatus.DoNotDisturb);
        }
    }
}