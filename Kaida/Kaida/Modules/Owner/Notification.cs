using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules.Owner
{
    [Group("Notification")]
    [Aliases("Notify", "Message", "Msg")]
    [RequireOwner]
    [Hidden]
    public class Notification : BaseCommandModule
    {
        private readonly ILogger logger;

        public Notification(ILogger logger)
        {
            this.logger = logger;
        }

        [Command("Owners")]
        public async Task NotifyOwners(CommandContext context, [RemainingText] string content)
        {
            var owner = context.User;
            var guilds = context.Client.Guilds.Values.ToList();

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Notification from {owner.Username}",
                Description = content,
                Color = DiscordColor.IndianRed,
                ThumbnailUrl = owner.AvatarUrl,
                Timestamp = DateTimeOffset.UtcNow
            };

            var contact = new StringBuilder().AppendLine($"Discord: `{owner.GetUsertag()}`")
                                             .AppendLine("[Join Developer Server](https://discord.gg/WgUDVAk)")
                                             .ToString();
            embed.AddField("Contact", contact);

            foreach (var guild in guilds)
            {
                embed.WithFooter($"This notification was sent to you due to you are the owner of the {guild.Name} server.");
                await guild.Owner.SendMessageAsync(embed: embed);
                logger.Information($"Notification from '{owner.GetUsertag()}' has been sent to the owner '{guild.Owner.GetUsertag()}' ({guild.Owner.Id}) from '{guild.Name}' ({guild.Id})");
            }
        }
    }
}