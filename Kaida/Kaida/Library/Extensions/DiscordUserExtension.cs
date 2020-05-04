using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaida.Library.Extensions
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
    }
}