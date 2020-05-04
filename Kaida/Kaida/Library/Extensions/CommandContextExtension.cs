using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Kaida.Library.Extensions
{
    public static class CommandContextExtension
    {
        /// <summary>
        ///     Deletes the response <see cref="DiscordMessage" /> after a specific time (in seconds).
        /// </summary>
        /// <param name="commandContext">Represents the <see cref="CommandContext" />.</param>
        /// <param name="content">The content which the bot will respond.</param>
        /// <param name="isTts">Weather the content is text-to-speech or not.</param>
        /// <param name="embed">Represents the <see cref="DiscordEmbed" />.</param>
        /// <param name="delay">The delay in which the respond will deleted.</param>
        /// <returns></returns>
        public static async Task RespondDeleteMessageDelayedAsync(this CommandContext commandContext, string content = null, bool isTts = false, DiscordEmbed embed = null, double delay = 10)
        {
            await commandContext.RespondAsync(content, isTts, embed);
            var botMessage = await commandContext.Channel.GetLastMessageAsync();
            await Task.Delay(TimeSpan.FromSeconds(delay));
            await commandContext.Channel.DeleteMessageAsync(botMessage);
        }
    }
}