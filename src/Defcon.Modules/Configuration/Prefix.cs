using System.Threading.Tasks;
using Defcon.Core.Entities.Discord.Embeds;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Configuration
{
    [Group("Prefix")]
    [Description("Shows the guilds command prefix.")]
    [RequireGuild]
    public class Prefix : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Prefix(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task ViewPrefix(CommandContext context)
        {
            var guild = await redis.GetAsync<Defcon.Data.Guilds.Guild>(RedisKeyNaming.Guild(context.Guild.Id));

            await context.RespondAsync($"The command prefix for this guild is {Formatter.InlineCode(guild.Prefix)}.");
        }

        [Command("Set")]
        [Description("Set the command prefix for this guild.")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetPrefix(CommandContext context, [Description("The new command prefix.")] string prefix)
        {
            var guild = await redis.GetAsync<Defcon.Data.Guilds.Guild>(RedisKeyNaming.Guild(context.Guild.Id));

            var oldPrefix = guild.Prefix;
            guild.Prefix = prefix;

            await redis.ReplaceAsync<Defcon.Data.Guilds.Guild>(RedisKeyNaming.Guild(context.Guild.Id), guild);

            var embed = new Embed {Title = "Command prefix set", Description = $"Set the prefix on the guild from {Formatter.InlineCode(oldPrefix)} to {Formatter.InlineCode(prefix)}."};

            this.logger.Information($"[Redis] Changed the prefix on the guild '{context.Guild.Name}' ({context.Guild.Id}) from '{oldPrefix}' to '{prefix}' by '{context.User.Mention}' ({context.User.Id}).");
            await context.SendEmbedMessageAsync(embed);
        }
    }
}