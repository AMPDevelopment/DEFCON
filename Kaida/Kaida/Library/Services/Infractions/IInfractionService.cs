using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Kaida.Data.Users;

namespace Kaida.Library.Services.Infractions
{
    public interface IInfractionService
    {
        Task CreateInfraction(DiscordGuild guild, DiscordChannel channel, DiscordClient client, DiscordMember moderator, DiscordMember suspect, string reason, InfractionType infractionType);
        Task ViewInfractions(DiscordGuild guild, DiscordChannel channel, DiscordMember suspect);
    }
}
