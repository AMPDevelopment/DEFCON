using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Kaida.Data.Guilds;
using Kaida.Data.Roles;
using Kaida.Library.Extensions;
using Kaida.Library.Redis;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Library.Services.Reactions
{
    public class ReactionService : IReactionService
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public ReactionService(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        public bool IsListener(ulong guildId, ulong messageId, DiscordEmoji emoji)
        {
            var guild = redis.GetAsync<Guild>(RedisKeyNaming.Guild(guildId)).Result;

            return guild.ReactionSingles != null && guild.ReactionSingles.Any(x => x.Id == messageId && x.ReactionItems.Any(x => x.Emoji == emoji.ToString())) ||
                   guild.ReactionMenus != null && guild.ReactionMenus.Any(x => x.Id == messageId && x.ReactionItems.Any(x => x.Emoji == emoji.ToString()));
        }

        public async void ManageRole(DiscordMessage message, DiscordChannel channel, DiscordMember member, DiscordEmoji emoji)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(channel.GuildId));

            var reactionSingleItem = guild.ReactionSingles?.Single(x => x.Id == message.Id)?.ReactionItems.Single(x => x.Emoji == emoji.ToString());
            var reactionMenuItem = guild.ReactionMenus?.Single(x => x.Id == message.Id)?.ReactionItems.Single(x => x.Emoji == emoji.ToString());

            var roleId = ulong.MinValue;
            if (reactionSingleItem != null)
            {
                roleId = reactionSingleItem.RoleId;
            }

            if (reactionMenuItem != null)
            {
                roleId = reactionMenuItem.RoleId;
            }

            var role = channel.Guild.GetRole(roleId);

            if (role != null)
            {
                if (member.Roles.Contains(role))
                {
                    logger.Information($"The role '{role.Name}' was revoked from '{member.GetUsertag()}' on the guild '{channel.Guild.Name}' ({channel.GuildId}).");
                    await member.RevokeRoleAsync(role);
                    await member.SendMessageAsync($"The role {Formatter.InlineCode(role.Name)} has been revoked from you on {Formatter.Bold(channel.Guild.Name)}.");
                }
                else
                {
                    logger.Information($"The role '{role.Name}' was granted to '{member.GetUsertag()}' on the guild '{channel.Guild.Name}' ({channel.GuildId}).");
                    await member.GrantRoleAsync(role);
                    await member.SendMessageAsync($"The role {Formatter.InlineCode(role.Name)} has been granted to you on {Formatter.Bold(channel.Guild.Name)}.");
                }

                await message.DeleteReactionAsync(emoji, member);
            }
        }

        public Task CreateReactionMenu(ulong guildId, ulong messageId, string name)
        {
            throw new NotImplementedException();
        }

        public async Task AddRoleToListener(ulong guildId, ulong messageId, DiscordEmoji emoji, DiscordRole role, ReactionType type)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guildId));
            
            if (type == ReactionType.Single)
            {
                guild.ReactionSingles ??= new List<ReactionSingle>();

                if (guild.ReactionSingles.Any(x => x.Id != messageId))
                {
                    guild.ReactionSingles.Add(new ReactionSingle()
                    {
                        Id = messageId,
                        ReactionItems = new List<ReactionItem>()
                    });
                }

                guild.ReactionSingles.Single(x => x.Id == messageId).ReactionItems.Add(new ReactionItem()
                {
                    Emoji = emoji.ToString(),
                    RoleId = role.Id
                });

                await redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(guildId), guild);

                await Task.CompletedTask;
            }
        }

        public Task RemoveRoleFromListener(ulong guildId, ulong messageId, DiscordEmoji emoji)
        {
            throw new NotImplementedException();
        }
    }
}