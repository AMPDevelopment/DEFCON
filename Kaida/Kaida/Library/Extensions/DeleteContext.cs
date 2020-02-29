using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Kaida.Library.Extensions
{
    public static class DeleteContext
    {
        public static async Task DeleteLastMessage(this DiscordChannel channel, double delay = 2)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay).Milliseconds);
            var lastMessage = await channel.GetMessagesAsync(1);
            await channel.DeleteMessagesAsync(lastMessage);
        }

        public static async Task DeleteLastMessages(this DiscordChannel channel, int amount)
        {
            var messages = await channel.GetMessagesAsync(amount);
            await channel.DeleteMessagesAsync(messages);
        }

        public static async Task DeleteMessageById(this DiscordChannel channel, ulong messageId)
        {
            await channel.GetMessageAsync(messageId).Result.DeleteAsync();
        }
    }
}
