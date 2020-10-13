using System.Threading.Tasks;
using Defcon.Core.Entities.Enums;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Defcon.Library.Services.Logs
{
    public interface ILogService
    {
        Task LogInit(CommandContext context, string status, LogType logType);
        Task GuildLogger(BaseDiscordClient client, DiscordGuild guild, object eventArgs, LogType logType);
    }
}
