using System.Threading.Tasks;
using Defcon.Data.Users;
using Defcon.Library.Services.Infractions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Configuration
{
    [Group("Moderators")]
    [RequireGuild]
    public class Moderators : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;
        private readonly IInfractionService infractionService;

        public Moderators(ILogger logger, IRedisDatabase redis, IInfractionService infractionService)
        {
            this.logger = logger;
            this.redis = redis;
            this.infractionService = infractionService;
        }

        [Command("Add")]
        public async Task AddModerator(CommandContext context, DiscordRole role)
        {
            await infractionService.ManageModerators(context.Guild, context.Channel, role, true);
        }
        
        [Command("Remove")]
        public async Task RemoveModerator(CommandContext context, DiscordRole role)
        {
            await infractionService.ManageModerators(context.Guild, context.Channel, role, false);
        }
    }
}