using System.Threading.Tasks;
using Defcon.Library.Attributes;
using Defcon.Library.Services.Infractions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Moderation
{
    [Category("Moderation")]
    [Group("Infractions")]
    [RequireGuild]
    public class Infractions : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;
        private readonly IInfractionService infractionService;

        public Infractions(ILogger logger, IRedisDatabase redis, IInfractionService infractionService)
        {
            this.logger = logger;
            this.redis = redis;
            this.infractionService = infractionService;
        }

        [GroupCommand]
        public async Task View(CommandContext context, [Description("The suspect.")] DiscordMember suspect = null)
        {
            suspect = suspect == null ? context.Member : suspect;
            await infractionService.ViewInfractions(context.Guild, context.Channel, suspect);
        }
    }
}