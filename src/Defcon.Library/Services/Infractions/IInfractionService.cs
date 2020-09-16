using System.Threading.Tasks;
using Defcon.Data.Users;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Defcon.Library.Services.Infractions
{
    public interface IInfractionService
    {
        Task CreateInfraction(DiscordGuild guild, DiscordChannel channel, DiscordClient client, DiscordMember moderator, DiscordMember suspect, string reason, InfractionType infractionType);
        Task ViewInfractions(DiscordGuild guild, DiscordChannel channel, DiscordMember suspect);
    }
}
