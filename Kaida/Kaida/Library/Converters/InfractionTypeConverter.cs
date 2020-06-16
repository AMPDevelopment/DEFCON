using System;
using Kaida.Data.Users;

namespace Kaida.Library.Converters
{
    public static class InfractionTypeConverter
    {
        public static string ToInfractionString(this InfractionType infractionType)
        {
            var value = string.Empty;

            switch (infractionType)
            {
                case InfractionType.AutoMod:
                    value = "Auto Moderation";
                    break;
                case InfractionType.Warning:
                    value = "Warn";
                    break;
                case InfractionType.Mute:
                    value = "Mute";
                    break;
                case InfractionType.Kick:
                    value = "Kick";
                    break;
                case InfractionType.Ban:
                    value = "Ban";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(infractionType), infractionType, null);
            }

            return value;
        }

        public static string ToActionString(this InfractionType infractionType)
        {
            var value = string.Empty;

            switch (infractionType)
            {
                case InfractionType.AutoMod:
                    value = "Auto Moderated";
                    break;
                case InfractionType.Warning:
                    value = "Warned";
                    break;
                case InfractionType.Mute:
                    value = "Muted";
                    break;
                case InfractionType.Kick:
                    value = "Kicked";
                    break;
                case InfractionType.Ban:
                    value = "Banned";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(infractionType), infractionType, null);
            }

            return value;
        }
    }
}