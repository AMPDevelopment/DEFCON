using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DEFCON.Library.Services.SyncedInfractions
{
    public interface ISyncedInfractions
    {
        Task SyncBans(DiscordGuild guild);
    }
}