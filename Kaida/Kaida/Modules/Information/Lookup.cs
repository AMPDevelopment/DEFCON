using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Attributes;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Information
{
    [Category("Information")]
    [Group("Lookup")]
    [Description("Shows you details about a user globally.")]
    public class Lookup : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Lookup(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task LookupSuspect(CommandContext context, [Description("The suspect.")] DiscordUser user)
        {
            var userDays = await user.GetDaysExisting();
            var userSinceDays =  userDays == 1 ? $"yesterday" : userDays == 0 ? "today" : $"{Formatter.Bold($"{userDays}")} days";
            var description = new StringBuilder().AppendLine($"Identity: {Formatter.InlineCode($"{user.Id}")}")
                                                 .AppendLine($"Registered: {await user.CreatedAtLongDateTimeString()} ({userSinceDays})")
                                                 .ToString();
            var embed = new Embed()
            {
                Author = new EmbedAuthor()
                {
                    Name = user.GetUsertag(),
                    IconUrl = user.AvatarUrl
                },
                ThumbnailUrl = user.AvatarUrl,
                Description = description
            };

            await context.SendEmbedMessageAsync(embed);
        }
    }
}
