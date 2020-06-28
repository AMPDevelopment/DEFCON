using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Attributes;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Information
{
    [Category("Information")]
    [Group("Ping")]
    [Description("Shows how good I am in Ping Pong.")]
    public class Ping : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public Ping(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task Pong(CommandContext context)
        {
            await context.RespondAsync($"{DiscordEmoji.FromGuildEmote(context.Client, EmojiLibrary.Ping)} {context.Client.Ping}ms");
        }
    }
}