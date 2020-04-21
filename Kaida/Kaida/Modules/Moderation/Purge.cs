using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Kaida.Library.Extensions;
using System.Linq;
using Serilog;

namespace Kaida.Modules.Moderation
{
    [Group("Purge")]
    [RequirePermissions(Permissions.ManageMessages)]
    [RequireBotPermissions(Permissions.ManageMessages)]
    public class Purge : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Purge(ILogger logger)
        {
            _logger = logger;
        }

        [GroupCommand]
        [Description("Purges an x amount of messages.")]
        public async Task PurgeMessages(CommandContext context, int amount, [RemainingText] string reason = null)
        {
            if (amount <= 5000)
            {
                var msgs = await context.Channel.GetMessagesBeforeAsync(context.Message.Id, amount);
                context.Channel.BulkMessagesAsync(msgs, reason);
                if (amount == 1)
                {
                    await context.RespondDeleteMessageDelayedAsync($"{context.User.Username} deleted 1 message.");
                }
                else
                {
                    await context.RespondDeleteMessageDelayedAsync($"{context.User.Username} deleted {amount} messages.");
                }
            }
            else
            {
                await context.RespondDeleteMessageDelayedAsync("You can't delete more than 5000 messages at once.");
            }
        }

        [GroupCommand]
        [Description("Purges all messages from the target within the x amount of messages.")]
        public async Task PurgeUserMessages(CommandContext context, DiscordUser user, int amount, [RemainingText] string reason = null)
        {
            if (amount <= 5000)
            {
                var msgs = context.Channel.GetMessagesBeforeAsync(context.Message.Id, amount).Result.Where(x => x.Author == user);
                context.Channel.BulkMessagesAsync(msgs, reason);

                var actuallyAmount = msgs.Count();
                if (actuallyAmount == 1)
                {
                    await context.RespondDeleteMessageDelayedAsync($"{context.User.Username} deleted 1 message from {user.Mention} in the last {amount} messages.");
                }
                else
                {
                    await context.RespondDeleteMessageDelayedAsync($"{context.User.Username} deleted {actuallyAmount} messages from {user.Mention} in the last {amount} messages.");
                }
            }
            else
            {
                await context.RespondDeleteMessageDelayedAsync("You can't delete more than 5000 messages at once.");
            }
        }

        [Command("Emotes")]
        public async Task PurgeEmotes(CommandContext context, ulong messageId, DiscordEmoji emoji, [RemainingText] string reason = "Unspecified")
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            var users = await message.GetReactionsAsync(emoji);
            foreach (var user in users)
            {
                await message.DeleteReactionAsync(emoji, user, reason);
                Task.Delay(500);
            }
        }
    }
}
