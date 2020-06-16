﻿using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Data.Users;
using Kaida.Library.Services.Infractions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Moderation
{
    [Group("Warn")]
    [RequireGuild]
    public class Warn : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;
        private readonly IInfractionService infractionService;

        public Warn(ILogger logger, IRedisDatabase redis, IInfractionService infractionService)
        {
            this.logger = logger;
            this.redis = redis;
            this.infractionService = infractionService;
        }

        [GroupCommand]
        [Priority(1)]
        public async Task WarnSuspect(CommandContext context, DiscordMember suspect, [RemainingText] string reason = "No reason given.")
        {
            await infractionService.CreateInfraction(context.Guild, context.Channel, context.Client, context.Member, suspect, reason, InfractionType.Warning);
        }

        [GroupCommand]
        [Priority(2)]
        public async Task WarnSuspect(CommandContext context, ulong suspectId, [RemainingText] string reason = "No reason given.")
        {
            var suspect = await context.Guild.GetMemberAsync(suspectId);
            await infractionService.CreateInfraction(context.Guild, context.Channel, context.Client, context.Member, suspect, reason, InfractionType.Warning);
        }
    }
}