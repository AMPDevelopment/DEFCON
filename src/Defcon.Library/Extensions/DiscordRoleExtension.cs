using System.Text;
using System.Threading.Tasks;
using DSharpPlus;

namespace Defcon.Library.Extensions
{
    public static class DiscordRoleExtension
    {
        public static async Task<string> GetChangedRolesDifference(this Permissions that, Permissions other)
        {
            var permissions = new StringBuilder();

            RolePermsDiffChecker(permissions, that, other, Permissions.Administrator);
            RolePermsDiffChecker(permissions, that, other, Permissions.ViewAuditLog);
            RolePermsDiffChecker(permissions, that, other, Permissions.ManageGuild);
            RolePermsDiffChecker(permissions, that, other, Permissions.ManageRoles);
            RolePermsDiffChecker(permissions, that, other, Permissions.ManageChannels);
            RolePermsDiffChecker(permissions, that, other, Permissions.KickMembers);
            RolePermsDiffChecker(permissions, that, other, Permissions.BanMembers);
            RolePermsDiffChecker(permissions, that, other, Permissions.CreateInstantInvite);
            RolePermsDiffChecker(permissions, that, other, Permissions.ChangeNickname);
            RolePermsDiffChecker(permissions, that, other, Permissions.ManageNicknames);
            RolePermsDiffChecker(permissions, that, other, Permissions.ManageEmojis);
            RolePermsDiffChecker(permissions, that, other, Permissions.ManageWebhooks);
            RolePermsDiffChecker(permissions, that, other, Permissions.AccessChannels);
            RolePermsDiffChecker(permissions, that, other, Permissions.SendMessages);
            RolePermsDiffChecker(permissions, that, other, Permissions.SendTtsMessages);
            RolePermsDiffChecker(permissions, that, other, Permissions.ManageMessages);
            RolePermsDiffChecker(permissions, that, other, Permissions.EmbedLinks);
            RolePermsDiffChecker(permissions, that, other, Permissions.AttachFiles);
            RolePermsDiffChecker(permissions, that, other, Permissions.ReadMessageHistory);
            RolePermsDiffChecker(permissions, that, other, Permissions.MentionEveryone);
            RolePermsDiffChecker(permissions, that, other, Permissions.UseExternalEmojis);
            RolePermsDiffChecker(permissions, that, other, Permissions.AddReactions);
            RolePermsDiffChecker(permissions, that, other, Permissions.UseVoice);
            RolePermsDiffChecker(permissions, that, other, Permissions.Speak);
            RolePermsDiffChecker(permissions, that, other, Permissions.Stream);
            RolePermsDiffChecker(permissions, that, other, Permissions.MuteMembers);
            RolePermsDiffChecker(permissions, that, other, Permissions.DeafenMembers);
            RolePermsDiffChecker(permissions, that, other, Permissions.MoveMembers);
            RolePermsDiffChecker(permissions, that, other, Permissions.UseVoiceDetection);
            RolePermsDiffChecker(permissions, that, other, Permissions.PrioritySpeaker);

            return permissions.ToString();
        }

        private static void RolePermsDiffChecker(StringBuilder builder, Permissions that, Permissions other, Permissions check)
        {
            var isThat = that.HasPermission(check);
            var isOther = other.HasPermission(check);

            if (isThat != isOther)
            {
                builder.AppendLine($"{check.ToPermissionString()}: {Formatter.InlineCode(isThat.ToString())} to {Formatter.InlineCode(isOther.ToString())}");
            }
        }
    }
}