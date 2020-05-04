using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis;

namespace Kaida.Modules
{
    [Group("Prefix")]
    [RequirePermissions(Permissions.ManageGuild)]
    public class Prefix : BaseCommandModule
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;

        public Prefix(ILogger logger, IDatabase redis)
        {
            _logger = logger;
            _redis = redis;
        }

        [Command("Change")]
        public async Task ChangePrefix(CommandContext context, string prefix)
        {
            var oldPrefix = _redis.StringGet($"{context.Guild.Id}:CommandPrefix");
            var description = $"Changed the prefix on the guild from {Formatter.InlineCode(oldPrefix)} to {Formatter.InlineCode(prefix)}.";
            _redis.StringSet($"{context.Guild.Id}:CommandPrefix", prefix);
            _logger.Information($"[Redis] Changed the prefix on the guild '{context.Guild.Name}' ({context.Guild.Id}) from '{oldPrefix}' to '{prefix}' by '{context.User.Mention}' ({context.User.Id}).");
            await context.EmbeddedMessage("Command prefix changed", description);
        }
    }
}