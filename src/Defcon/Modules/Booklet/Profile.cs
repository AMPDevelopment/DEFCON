using System.Text;
using System.Threading.Tasks;
using Defcon.Data.Users;
using Defcon.Entities.Discord.Embeds;
using Defcon.Library.Attributes;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Booklet
{
    [Category("Booklet")]
    [Group("Profile")]
    public class Profile : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;
        private User userData;

        public Profile(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task View(CommandContext context, DiscordUser targetUser = null)
        {
            var user = targetUser == null ? context.User : targetUser;

            userData = await redis.GetAsync<User>(RedisKeyNaming.User(user.Id));

            var description = new StringBuilder().AppendLine($"Description: {userData.Description}").ToString();


            var embed = new Embed()
            {
                Author = new EmbedAuthor()
                {
                    Name = user.Username
                },
                Description = description
            };

            await context.SendEmbedMessageAsync(embed);
        }

        [Command("Description")]
        public async Task SetDescription(CommandContext context, [RemainingText] string description)
        {
            var user = context.User;
            userData = await redis.GetAsync<User>(RedisKeyNaming.User(user.Id));

            userData.Description = description;
            await redis.ReplaceAsync(RedisKeyNaming.User(user.Id), userData);
        }
    }
}