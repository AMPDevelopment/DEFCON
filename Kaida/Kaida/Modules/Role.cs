using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Reaction;
using Serilog;

namespace Kaida.Modules
{
    [Group("Role")]
    [RequirePermissions(Permissions.ManageMessages)]
    public class Role : BaseCommandModule
    {
        private readonly ILogger _logger;
        private readonly IReactionListener _reactionListener;

        public Role(IReactionListener reactionListener, ILogger logger)
        {
            _reactionListener = reactionListener;
            _logger = logger;
        }

        [Command("Listen")]
        public async Task Listen(CommandContext context, ulong messageId, DiscordEmoji emoji, DiscordRole role)
        {
            await Cleanup(context, messageId, emoji);
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.CreateReactionAsync(emoji);
            await _reactionListener.AddRoleToListener(messageId, emoji, role, context.Client);
        }

        [Command("Unlisten")]
        public async Task Unlisten(CommandContext context, ulong messageId, DiscordEmoji emoji)
        {
            await _reactionListener.RemoveRoleFromListener(messageId, emoji, context.Client);
            await Cleanup(context, messageId, emoji);
        }

        [Command("Cleanup")]
        public async Task CleanEmojis(CommandContext context, ulong messageId, DiscordEmoji emoji)
        {
            await Cleanup(context, messageId, emoji);
        }

        private async Task Cleanup(CommandContext context, ulong messageId, DiscordEmoji emoji)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            var usersReacted = await message.GetReactionsAsync(emoji);
            foreach (var user in usersReacted)
            {
                await message.DeleteReactionAsync(emoji, user);
            }
        }
    }
}
