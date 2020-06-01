using System;
using Kaida.Data.Users;

namespace Kaida.Library.Converters
{
    public static class InfractionTypeConverter
    {
        public static string ToString(this InfractionType infractionType)
        {
            var value = string.Empty;

            switch (infractionType)
            {
                case InfractionType.AutoMod:
                    value = "Auto Moderation";
                    break;
                case InfractionType.Warning:
                    value = "Warning";
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
    }
}