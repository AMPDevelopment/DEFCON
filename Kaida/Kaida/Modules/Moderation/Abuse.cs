using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using Kaida.Library.Extensions;
using Serilog;
using System;

namespace Kaida.Modules.Moderation
{
    [Group("Abuse")]
    [RequirePermissions(Permissions.ManageMessages)]
    [RequireBotPermissions(Permissions.ManageMessages)]
    public class Abuse : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Abuse(ILogger logger)
        {
            _logger = logger;
        }

        [Command("Spam")]
        public async Task Spam(CommandContext context, int amount, [RemainingText] string text)
        {
            var duration = TimeSpan.FromSeconds(amount * 1.2);
            await context.RespondAsync($"**Abuse spam ETA: `{duration.ToString(@"hh\:mm\:ss")}`**");
            for (int i = 0; i < amount; i++)
            {
                await context.RespondAsync(text);
            }
        }
    }
}
