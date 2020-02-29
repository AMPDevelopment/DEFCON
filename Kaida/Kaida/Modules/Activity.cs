using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Serilog;
using StackExchange.Redis;

namespace Kaida.Modules
{
    [RequireOwner]
    public class Activity : BaseCommandModule
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;

        public Activity(ILogger logger, IDatabase redis)
        {
            _logger = logger;
            _redis = redis;
        }

        [Command("Activity")]
        public async Task Change(CommandContext context, [RemainingText] string content)
        {
            var activity = new DiscordActivity();

            var items = content.Split("|");
            activity.Name = items[1].Trim();
            switch (items[0].ToLowerInvariant().Trim())
            {
                case "custom":
                    activity.ActivityType = ActivityType.Custom;
                    break;
                case "playing":
                    activity.ActivityType = ActivityType.Playing;
                    break;
                case "watching":
                    activity.ActivityType = ActivityType.Watching;
                    break;
                case "streaming":
                    activity.StreamUrl = items[2].Trim();
                    activity.ActivityType = ActivityType.Streaming;
                    break;
                case "listening":
                    activity.ActivityType = ActivityType.ListeningTo;
                    break;
                default:
                    activity.ActivityType = ActivityType.Custom;
                    break;
            }

            await context.Client.UpdateStatusAsync(activity);
        }
    }
}
