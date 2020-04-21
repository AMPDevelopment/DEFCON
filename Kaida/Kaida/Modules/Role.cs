using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
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
        [Aliases("l")]
        public async Task Listen(CommandContext context, ulong messageId, DiscordEmoji emoji, DiscordRole role)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.CreateReactionAsync(emoji);
            await _reactionListener.AddRoleToListener(messageId, emoji, role, context.Client);
        }

        [Command("Unlisten")]
        [Aliases("ul")]
        public async Task Unlisten(CommandContext context, ulong messageId, DiscordEmoji emoji)
        {
            await _reactionListener.RemoveRoleFromListener(messageId, emoji, context.Client);
            await Cleanup(context, messageId, emoji);
        }


        [Command("AddCategory")]
        [Aliases("addcat")]
        public async Task AddCategory(CommandContext context, [RemainingText] string name)
        {
            
        }

        [Command("DeleteCategory")]
        [Aliases("delcat")]
        public async Task DeleteCategory(CommandContext context, int id)
        {

        }

        [Command("Reset")]
        [Aliases("ereset")]
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
                if (!user.IsBot) await message.DeleteReactionAsync(emoji, user);
                Task.Delay(500);
            }
        }
    }
}
