using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis;

namespace Kaida.Modules
{
    [Group("Prefix")]
    [RequirePermissions(Permissions.ManageGuild)]
    public class Prefix : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IDatabase redis;

        public Prefix(ILogger logger, IDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        public async Task ViewPrefix(CommandContext context)
        {
            var prefix = redis.StringGet($"{context.Guild.Id}:CommandPrefix");

            var embed = new Embed {Title = $"Command prefix for {context.Guild.Name}", Description = $"The command prefix for this guild is {Formatter.InlineCode(prefix)}."};

            await context.SendEmbedMessageAsync(embed);
        }

        [Command("Set")]
        public async Task SetPrefix(CommandContext context, string prefix)
        {
            var oldPrefix = redis.StringGet($"{context.Guild.Id}:CommandPrefix");
            redis.StringSet($"{context.Guild.Id}:CommandPrefix", prefix);

            var embed = new Embed {Title = "Command prefix set", Description = $"Set the prefix on the guild from {Formatter.InlineCode(oldPrefix)} to {Formatter.InlineCode(prefix)}."};

            logger.Information($"[Redis] Changed the prefix on the guild '{context.Guild.Name}' ({context.Guild.Id}) from '{oldPrefix}' to '{prefix}' by '{context.User.Mention}' ({context.User.Id}).");
            await context.SendEmbedMessageAsync(embed);
        }
    }
}