using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules.Moderation
{
    [RequirePermissions(Permissions.ManageMessages)]
    [RequireBotPermissions(Permissions.ManageMessages)]
    public class Purge : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Purge(ILogger logger)
        {
            _logger = logger;
        }

        [Command("Purge")]
        [Priority(1)]
        public async Task PurgeMessages(CommandContext context, int amount)
        {
            if (amount <= 1000)
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

                var lastBotMessageId = await context.Channel.GetMessagesAsync(1);
                await Task.Delay(5000);
                await context.Channel.DeleteMessagesAsync(lastBotMessageId);
            }
            else
            {
                await context.RespondAsync("You can't delete more than 1000 messages at once.");
            }
        }

        [Command("Emotes")]
        public async Task PurgeEmotes(CommandContext context, ulong messageId, DiscordEmoji emoji)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            var users = await message.GetReactionsAsync(emoji);

            foreach (var user in users)
            {
                await message.DeleteReactionAsync(emoji, user);
            }
        }
    }
}
