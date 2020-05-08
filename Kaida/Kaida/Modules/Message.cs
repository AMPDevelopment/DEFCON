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
    public class Message : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IDatabase redis;

        public Message(ILogger logger, IDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [Command("Say")]
        [Priority(1)]
        public async Task Say(CommandContext context, [RemainingText] string content)
        {
            await context.RespondAsync(content);
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
        [RequireBotPermissions(Permissions.EmbedLinks)]
        public async Task Embed(CommandContext context, [RemainingText] string content)
        {
            await context.SendFilteredEmbedMessageAsync(content);
        }

        [Command("Embed")]
        [Priority(2)]
        [RequireBotPermissions(Permissions.EmbedLinks)]
        public async Task Embed(CommandContext context, ulong messageId, [RemainingText] string content)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.SendFilteredEmbedMessageAsync(content);
        }
    }
}