using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Data.Users;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Attributes;
using Kaida.Library.Extensions;
using Kaida.Library.Redis;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Booklet
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