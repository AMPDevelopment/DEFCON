using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Attributes;
using Kaida.Library.Services.Infractions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Moderation
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
        [Priority(1)]
        public async Task View(CommandContext context, DiscordMember suspect = null)
        {
            suspect = suspect == null ? context.Member : suspect;
            await infractionService.ViewInfractions(context.Guild, context.Channel, suspect);
        }

        [GroupCommand]
        [Priority(2)]
        public async Task View(CommandContext context, ulong suspectId)
        {
            var suspect = await context.Guild.GetMemberAsync(suspectId);
            await infractionService.ViewInfractions(context.Guild, context.Channel, suspect);
        }
    }
}