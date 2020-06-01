using System.Collections.Generic;
using DSharpPlus;

namespace Kaida.Library.Extensions
{
    public static class BaseDiscordClientExtension
    {
        public static string GenerateInviteLink(this BaseDiscordClient client)
        {
            return $"https://discord.com/oauth2/authorize?client_id={client.CurrentApplication.Id}&scope=bot&permissions={PermissionCalc()}";
        }

        private static int PermissionCalc()
        {
            var permCalc = 0;
            var perms = new List<Permissions>()
            {
                Permissions.ManageRoles,
                Permissions.ManageChannels,
                Permissions.ManageMessages,
                Permissions.ManageNicknames,
                Permissions.ViewAuditLog,
                Permissions.KickMembers,
                Permissions.BanMembers,
                Permissions.ReadMessageHistory,
                Permissions.SendMessages,
                Permissions.EmbedLinks,
                Permissions.AddReactions,
                Permissions.UseExternalEmojis,
                Permissions.Speak,
                Permissions.UseVoice,
                Permissions.MuteMembers,
                Permissions.MoveMembers,
                Permissions.AttachFiles,
            };

            foreach (var perm in perms)
            {
                permCalc += perm.GetHashCode();
            }

            return permCalc;
        } 
    }
}