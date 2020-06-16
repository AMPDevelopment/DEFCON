using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;

namespace Kaida.Modules.Owner
{
    [RequireOwner]
    [Hidden]
    public class Holding : BaseCommandModule
    {
        private readonly ILogger logger;

        public Holding(ILogger logger)
        {
            this.logger = logger;
        }

        [Command("Leave")]
        public async Task Leave(CommandContext context, ulong guildId)
        {
            var guild = await context.Client.GetGuildAsync(guildId);

            if (guild != null)
            {
                var guildName = guild.Name;
                var guildMemberCount = guild.MemberCount;
                await guild.LeaveAsync();
                await context.RespondAsync($"Successfully left {Formatter.InlineCode(guildName)} with {guildMemberCount} members.");
            }
        }
    }
}
