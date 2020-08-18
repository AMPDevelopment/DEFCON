using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Defcon.Entities.Discord.Embeds;
using Defcon.Library.Attributes;
using Defcon.Library.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;
using SteamCSharp;

namespace Defcon.Modules.Games
{
    [Category("Games")]
    [Group("CSGO")]
    [Description("Steam csgo stats.")]
    public class CounterStrike : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;
        private readonly Steam steam;

        public CounterStrike(ILogger logger, IRedisDatabase redis, Steam steam)
        {
            this.logger = logger;
            this.redis = redis;
            this.steam = steam;
        }

        [Command("Stats")]
        public async Task Stats(CommandContext context, string steamId)
        {
            try
            {
                var steamUser = await steam.GetSteamUserAsync(steamId);

                var embed = new Embed()
                {
                    Author = new EmbedAuthor()
                    {
                        Name = steamUser.PersonaName,
                        IconUrl = steamUser.AvatarFullUrl,
                        Url = steamUser.ProfileUrl
                    },
                    ThumbnailUrl = steamUser.AvatarFullUrl,
                    Description = "Nothing"
                };

                await context.SendEmbedMessageAsync(embed);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }
    }
}
