using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Services.Reactions;
using Serilog;

namespace Kaida.Modules.Configuration
{
    [Group("Role")]
    [RequirePermissions(Permissions.ManageMessages)]
    [RequireGuild]
    public class Role : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IReactionService reactionService;

        public Role(ILogger logger, IReactionService reactionService)
        {
            this.logger = logger;
            this.reactionService = reactionService;
        }

        [Command("Listen")]
        [Aliases("L")]
        public async Task Listen(CommandContext context, ulong messageId, DiscordEmoji emoji, DiscordRole role)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.CreateReactionAsync(emoji);
            await reactionService.AddRoleToListener(context.Guild.Id, messageId, emoji, role, ReactionType.Single);
        }

        [Command("Unlisten")]
        [Aliases("UL")]
        public async Task Unlisten(CommandContext context, ulong messageId, DiscordEmoji emoji)
        {
            await Cleanup(context, messageId, emoji);
        }

        [Command("AddCategory")]
        [Aliases("AddCat")]
        public async Task AddCategory(CommandContext context, [RemainingText] string name)
        {
        }

        [Command("DeleteCategory")]
        [Aliases("DelCat")]
        public async Task DeleteCategory(CommandContext context, int id)
        {
        }

        [Command("Reset")]
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