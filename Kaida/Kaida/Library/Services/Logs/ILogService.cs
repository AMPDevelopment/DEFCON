using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Kaida.Common.Enums;

namespace Kaida.Library.Services.Logs
{
    public interface ILogService
    {
        Task LogInit(CommandContext context, string status, LogType logType);
        Task GuildLogger(DiscordGuild guild, object eventArgs, LogType logType);
    }
}
