using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Defcon.Library.Redis;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace DEFCON.Library.Services.SyncedInfractions
{
    public class SyncedInfractions : ISyncedInfractions
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public SyncedInfractions(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        public async Task SyncBans(DiscordGuild guild)
        {
            var syncedBans = await redis.GetAsync<List<ulong>>(RedisKeyNaming.SyncedInfractions);

            foreach (var bannedUser in syncedBans)
            {
                await guild.BanMemberAsync(bannedUser, 0, "Synced with the database which contains all reported users (banned)");
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}