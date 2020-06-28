using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules.Moderation
{
    [Group("Purge")]
    [Aliases("Clear")]
    [RequirePermissions(Permissions.ManageMessages)]
    [RequireGuild]
    public class Purge : BaseCommandModule
    {
        private readonly ILogger logger;

        public Purge(ILogger logger)
        {
            this.logger = logger;
        }

        [GroupCommand]
        [Description("Purges an x amount of messages.")]
        [Priority(1)]
        public async Task PurgeMessages(CommandContext context, int amount, [RemainingText] string reason = "No reason given.")
        {
            var response = await context.RespondAsync($"Deleting {amount} message(s)");
            if (amount <= 5000 && amount >= 1)
            {
                var messages = await context.Channel.GetMessagesBeforeAsync(context.Message.Id, amount);
                await context.Channel.BulkMessagesAsync(messages, reason);
                
                if (amount == 1)
                {
                    await response.ModifyAsync($"{context.User.Username} deleted 1 message.");
                }
                else
                {
                    await response.ModifyAsync($"{context.User.Username} deleted {amount} messages.");
                }
            }
            else
            {
                await response.ModifyAsync("You can't delete more than 5000 messages at once.");
            }
        }

        [GroupCommand]
        [Description("Purges all messages from the target within the x amount of messages.")]
        [Priority(2)]
        public async Task PurgeUserMessages(CommandContext context, DiscordUser user, int amount, [RemainingText] string reason = "No reason given.")
        {
            var response = await context.RespondAsync($"Deleting {amount} message(s)");
            if (amount <= 5000)
            {
                var messages = context.Channel.GetMessagesBeforeAsync(context.Message.Id, amount)
                                      .Result.Where(x => x.Author == user)
                                      .ToList();
                context.Channel.BulkMessagesAsync(messages, reason);

                var actuallyAmount = messages.Count;

                if (actuallyAmount == 1)
                {
                    await response.ModifyAsync($"{context.User.Username} deleted 1 message from {user.Mention} in the last {amount} messages.");
                }
                else
                {
                    await response.ModifyAsync($"{context.User.Username} deleted {actuallyAmount} messages from {user.Mention} in the last {amount} messages.");
                }
            }
            else
            {
                await response.ModifyAsync("You can't delete more than 5000 messages at once.");
            }
        }

        [Command("Emotes")]
        public async Task PurgeEmotes(CommandContext context, ulong messageId, DiscordEmoji emoji, [RemainingText] string reason = "No reason given.")
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.DeleteReactionsEmojiAsync(emoji);
        }
    }
}