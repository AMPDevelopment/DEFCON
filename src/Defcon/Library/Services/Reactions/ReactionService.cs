using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Defcon.Data.Guilds;
using Defcon.Data.Roles;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Library.Services.Reactions
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

            return guild.ReactionCategories != null && guild.ReactionCategories.Any(x => x.Id == messageId && x.ReactionRoles.Any(x => x.Emoji == emoji.ToString())) ||
                   guild.ReactionMessages != null && guild.ReactionMessages.Any(x => x.Id == messageId && x.ReactionRoles.Any(x => x.Emoji == emoji.ToString()));
        }

        public async void ManageRole(DiscordMessage message, DiscordChannel channel, DiscordMember member, DiscordEmoji emoji)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(channel.GuildId));

            var reactionCategory = guild.ReactionCategories?.FirstOrDefault(x => x.Id == message.Id);
            var reactionCategoryItem = reactionCategory?.ReactionRoles.Single(x => x.Emoji == emoji.ToString());
            var reactionMessageItem = guild.ReactionMessages?.Single(x => x.Id == message.Id)?.ReactionRoles.Single(x => x.Emoji == emoji.ToString());

            var roleId = ulong.MinValue;
            var categoryRoleId = ulong.MinValue;

            if (reactionCategoryItem != null)
            {
                categoryRoleId = reactionCategory.RoleId;
                roleId = reactionCategoryItem.RoleId;
            }

            if (reactionMessageItem != null)
            {
                roleId = reactionMessageItem.RoleId;
            }

            var role = channel.Guild.GetRole(roleId);
            var categoryRole = channel.Guild.GetRole(categoryRoleId);

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

        public async Task CreateReactionCategory(ulong guildId, ulong messageId, string name, ulong roleId)
        {
            throw new NotImplementedException();
        }

        public async Task AddReactionListener(ulong guildId, ulong messageId, DiscordEmoji emoji, DiscordRole role, ReactionType type)
        {
            var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(guildId));
            
            if (type == ReactionType.Message)
            {
                guild.ReactionMessages ??= new List<ReactionMessage>();

                if (guild.ReactionMessages.Any(x => x.Id != messageId))
                {
                    guild.ReactionMessages.Add(new ReactionMessage()
                    {
                        Id = messageId,
                        ReactionRoles = new List<ReactionRole>()
                    });
                }

                guild.ReactionMessages.Single(x => x.Id == messageId).ReactionRoles.Add(new ReactionRole()
                {
                    Emoji = emoji.ToString(),
                    RoleId = role.Id
                });

                await redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(guildId), guild);

                await Task.CompletedTask;
            }

            if (type == ReactionType.Category)
            {
                guild.ReactionCategories ??= new List<ReactionCategory>();

                if (guild.ReactionCategories.Any(x => x.Id != messageId))
                {
                    guild.ReactionCategories.Add(new ReactionCategory()
                    {
                        Id = messageId,
                        RoleId = ulong.MinValue,
                        ReactionRoles = new List<ReactionRole>()
                    });
                }

                guild.ReactionMessages.Single(x => x.Id == messageId).ReactionRoles.Add(new ReactionRole()
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