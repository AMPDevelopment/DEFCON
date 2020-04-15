using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis;

namespace Kaida.Modules
{
    [RequirePermissions(Permissions.ManageMessages)]
    public class Texting : BaseCommandModule
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;

        public Texting(ILogger logger, IDatabase redis)
        {
            _logger = logger;
            _redis = redis;
        }

        [Command("Say")]
        [Priority(1)]
        public async Task Say(CommandContext context, [RemainingText] string content)
        {
            await context.Channel.SendMessageAsync(content);
        }

        [Command("Say")]
        [Priority(2)]
        public async Task Say(CommandContext context, ulong messageId, [RemainingText] string content)
        {
            if (await context.Channel.GetMessageAsync(messageId) is DiscordMessage message)
            {
                await message.ModifyAsync(content);
            }
        }

        [Command("Embed")]
        [Priority(1)]
        public async Task Embed(CommandContext context, [RemainingText] string content)
        {
            await context.EmbeddedFilteredMessage(content);
        }

        [Command("Embed")]
        [Priority(2)]
        public async Task Embed(CommandContext context, ulong messageId, [RemainingText] string content)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.EmbeddedFilteredMessage(content);
        }
    }
}
