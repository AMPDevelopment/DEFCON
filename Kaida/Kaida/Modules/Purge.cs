using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules
{
    public class Purge : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Purge(ILogger logger)
        {
            _logger = logger;
        }

        [Command("Purge")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PurgeMessages(CommandContext context, int amount)
        {
            if (amount <= 100)
            {
                await context.Channel.DeleteLastMessages(amount + 1);
                if (amount == 1)
                {
                    await context.RespondAsync($"{context.User.Username} deleted 1 message.");
                }
                else
                {
                    await context.RespondAsync($"{context.User.Username} deleted {amount} messages.");
                }

                await context.Channel.DeleteLastMessage(5);
            }
            else
            {
                await context.RespondAsync("You can't delete more than 100 messages at once.");
            }
        }
    }
}
