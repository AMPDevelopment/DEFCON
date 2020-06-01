using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Kaida.Data.Guilds;
using Kaida.Data.Roles;
using Kaida.Data.Users;
using Kaida.Library.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Library.Extensions
{
    public static class RedisDatabaseExtension
    {
        public static async Task<User> InitUser(this IRedisDatabase redis, ulong userId)
        {
            var user = await redis.GetAsync<User>(RedisKeyNaming.User(userId));

            if (user != null) return user;

            user = new User
            {
                Id = userId,
                Description = null,
                Birthdate = null,
                SteamId = null,
                Nicknames = new List<Nickname>(),
                Infractions = new List<Infraction>(),
                InfractionId = 0
            };
                    
            await redis.AddAsync(RedisKeyNaming.User(userId), user);

            return user;
        }

        public static async Task<bool> IsModerator(this IRedisDatabase redis, ulong guildId, DiscordMember member)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guildId));

            var isModerator = false;

            foreach (var role in member.Roles)
            {
                if (isModerator)
                {
                    break;
                }

                isModerator = guild.ModeratorRoleIds.Any(x => x == role.Id);
            }

            return isModerator;
        }

        public static async Task InitGuild(this IRedisDatabase redis, ulong guildId)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guildId));

            if (guild == null)
            {
                await redis.AddAsync<Guild>(RedisKeyNaming.Guild(guildId), new Guild()
                {
                    Id = guildId,
                    Prefix = ApplicationInformation.DefaultPrefix,
                    ModeratorRoleIds = new List<ulong>(),
                    ModeratorAllowedWarn = true,
                    ModeratorAllowedMute = true,
                    ModeratorAllowedKick = false,
                    ModeratorAllowedBan = false,
                    Logs = new List<Log>(),
                    Settings = new List<Setting>(),
                    ReactionSingles = new List<ReactionSingle>(),
                    ReactionMenus = new List<ReactionMenu>()
                });
            }
        }
    }
}