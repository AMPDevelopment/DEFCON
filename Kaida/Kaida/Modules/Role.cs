using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Reaction;
using Serilog;

namespace Kaida.Modules
{
    [Group("Role")]
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
        public async Task Listen(CommandContext context, string messageId, DiscordEmoji emoji, DiscordRole role)
        {
            var message = await context.Channel.GetMessageAsync(ulong.Parse(messageId));
            await message.CreateReactionAsync(emoji);
            await _reactionListener.AddRoleToListener(messageId, emoji, role, context.Client);
        }
    }
}
