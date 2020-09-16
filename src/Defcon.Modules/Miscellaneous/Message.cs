using System.Threading.Tasks;
using Defcon.Library.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Miscellaneous
{
    [RequirePermissions(Permissions.ManageMessages)]
    [Description("Let the bot write your message for you.")]
    [RequireGuild]
    public class Message : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Message(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [Command("Say")]
        [Priority(1)]
        public async Task Say(CommandContext context, [RemainingText] string content)
        {
            await context.Message.DeleteAsync();
            await context.RespondAsync(content);
        }

        [Command("Say")]
        [Priority(2)]
        public async Task Say(CommandContext context, ulong messageId, [RemainingText] string content)
        {
            if (await context.Channel.GetMessageAsync(messageId) is DiscordMessage message)
            {
                await context.Message.DeleteAsync();
                await message.ModifyAsync(content);
            }
        }

        [Command("Embed")]
        [Priority(1)]
        [RequireBotPermissions(Permissions.EmbedLinks)]
        public async Task Embed(CommandContext context, [RemainingText] string content)
        {
            await context.Message.DeleteAsync();
            await context.SendJsonEmbedMessageAsync(content);
        }

        [Command("Embed")]
        [Priority(2)]
        [RequireBotPermissions(Permissions.EmbedLinks)]
        public async Task Embed(CommandContext context, ulong messageId, [RemainingText] string content)
        {
            await context.Message.DeleteAsync();
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.EditJsonEmbedMessageAsync(content);
        }
    }
}