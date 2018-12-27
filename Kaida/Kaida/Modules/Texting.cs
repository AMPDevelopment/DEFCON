using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules
{
    public class Texting : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Texting(ILogger logger)
        {
            _logger = logger;
        }

        [Command("Say")]
        [Priority(1)]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Say(CommandContext context, [RemainingText] string content)
        {
            await context.Channel.SendMessageAsync(content);
        }

        [Command("Say")]
        [Priority(2)]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Say(CommandContext context, ulong messageId, [RemainingText] string content)
        {
            if (await context.Channel.GetMessageAsync(messageId) is DiscordMessage message)
            {
                await message.ModifyAsync(content);
            }
        }

        [Command("Embed")]
        [Priority(1)]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Embed(CommandContext context, [RemainingText] string content)
        {
            await context.EmbeddedFilteredMessage(content);
        }

        [Command("Embed")]
        [Priority(2)]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Embed(CommandContext context, ulong messageId, [RemainingText] string content)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.EmbeddedFilteredMessage(content);
        }
    }
}
