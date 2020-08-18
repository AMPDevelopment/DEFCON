using System.Threading.Tasks;
using Defcon.Library.Services.Reactions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;

namespace Defcon.Modules.Configuration
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

        [Command("Message")]
        public async Task Message(CommandContext context, ulong messageId, DiscordEmoji emoji, DiscordRole role)
        {
            var message = await context.Channel.GetMessageAsync(messageId);
            await message.CreateReactionAsync(emoji);
            await reactionService.AddReactionListener(context.Guild.Id, messageId, emoji, role, ReactionType.Message);
        }

        [Command("Unmessage")]
        public async Task Unmessage(CommandContext context, ulong messageId, DiscordEmoji emoji)
        {
            
        }
    }
}