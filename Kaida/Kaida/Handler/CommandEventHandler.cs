using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Attributes;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Handler
{
    public class CommandEventHandler
    {
        private readonly CommandsNextExtension commandsNext;
        private readonly ILogger logger;

        public CommandEventHandler(CommandsNextExtension commandsNext, ILogger logger)
        {
            this.commandsNext = commandsNext;
            this.logger = logger;

            this.commandsNext.CommandExecuted += CommandExecuted;
            this.commandsNext.CommandErrored += CommandErrored;
        }

        private async Task CommandExecuted(CommandExecutionEventArgs e)
        {
            var command = e.Command;
            var context = e.Context;
            var guild = context.Guild;
            var channel = context.Channel;
            var message = context.Message;
            var user = context.User;

            if (!channel.IsPrivate)
            {
                if (channel != null && user != null)
                {
                    logger.Information($"The command '{command.QualifiedName}' has been executed by '{user.GetUsertag()}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");
                    await channel.DeleteMessageByIdAsync(message.Id);
                }
                else
                {
                    logger.Information($"The command '{command.QualifiedName}' has been executed by an unknown user in a deleted channel on a unknown guild.");
                }
            }
            else
            {
                logger.Information($"The command '{command.QualifiedName}' has been executed by '{user.GetUsertag()}' ({user.Id}) in the direct message.");
            }
        }

        private async Task CommandErrored(CommandErrorEventArgs e)
        {
            var command = e.Command;
            var commandName = command?.QualifiedName ?? "unknown command";
            var context = e.Context;
            var guild = context.Guild;
            var channel = context.Channel;
            var user = context.User;

            if (e.Exception is ChecksFailedException checksFailedException)
            {
                context = checksFailedException.Context;
                var failedChecks = checksFailedException.FailedChecks;

                var permissions = failedChecks.OfType<RequirePermissionsAttribute>()
                                              .Select(x => x.Permissions.ToPermissionString());

                var userPermissions = failedChecks.OfType<RequireUserPermissionsAttribute>()
                                                  .Select(x => x.Permissions.ToPermissionString());

                var botPermissions = failedChecks.OfType<RequireBotPermissionsAttribute>()
                                                 .Select(x => x.Permissions.ToPermissionString());

                var requiredPermissionsDetails = new StringBuilder();

                if (failedChecks.Any(x => x is RequireOwnerAttribute))
                {
                    requiredPermissionsDetails.AppendLine("Owner-only");
                }

                if (failedChecks.Any(x => x is RequirePrivilegedUserAttribute))
                {
                    requiredPermissionsDetails.AppendLine("Privileged users only");
                }

                if (permissions.Any())
                {
                    requiredPermissionsDetails.AppendLine(Formatter.InlineCode(string.Join(",", permissions)));
                }

                if (userPermissions.Any())
                {
                    requiredPermissionsDetails.AppendLine($"User: {Formatter.InlineCode(string.Join(",", userPermissions))}");
                }

                if (botPermissions.Any())
                {
                    requiredPermissionsDetails.AppendLine($"Bot: {Formatter.InlineCode(string.Join(",", botPermissions))}");
                }

                if (botPermissions.Any(x => x.Equals(Permissions.EmbedLinks.ToPermissionString())))
                {
                    await context.RespondAsync($":no_entry: Access denied\nThe bot is requiring the following permission: {Formatter.InlineCode(Permissions.EmbedLinks.ToPermissionString())}");
                }
                else
                {
                    var embed = new Embed
                    {
                        Title = ":no_entry: Access denied",
                        Description = "You do not have the permissions required to execute this command.",
                        Color = DiscordColor.IndianRed,
                        Fields = new List<EmbedField> {new EmbedField {Name = "Required Permissions", Value = requiredPermissionsDetails.ToString(), Inline = false}},
                        Footer = new EmbedFooter {Text = $"Command: {commandName} | Requested by {user.GetUsertag()} | {user.Id}", IconUrl = user.AvatarUrl}
                    };

                    await context.SendEmbedMessageAsync(embed);
                }
            }

            if (e.Exception is CommandNotFoundException commandNotFoundException)
            {
                var failedCommand = commandNotFoundException.CommandName;
                var embed = new Embed {Title = ":no_entry: Command not found", Description = $"The command {Formatter.InlineCode(failedCommand)} does not exist.", Color = DiscordColor.Aquamarine, Footer = new EmbedFooter {Text = $"Requested on {guild.Name} | {guild.Id}", IconUrl = guild.IconUrl}};

                await guild.GetMemberAsync(user.Id)
                           .Result.SendEmbedMessageAsync(embed);
            }

            if (e.Exception is ArgumentException argumentException)
            {
                var embed = new Embed
                {
                    Title = ":no_entry: Argument Exception",
                    Description = $"{argumentException.Message}",
                    Color = DiscordColor.Aquamarine,
                    Fields = new List<EmbedField> {new EmbedField {Name = "Command Example", Value = Formatter.InlineCode($"{commandName} SOON AVAILABLE")}},
                    Footer = new EmbedFooter {Text = $"Requested on {guild.Name} | {guild.Id}", IconUrl = guild.IconUrl}
                };

                await guild.GetMemberAsync(user.Id)
                           .Result.SendEmbedMessageAsync(embed);
            }

            if (e.Exception is InvalidOperationException invalidOperationException)
            {
                var embed = new Embed {Title = ":no_entry: Invalid Operation", Description = $"{invalidOperationException.Message}", Color = DiscordColor.Aquamarine, Footer = new EmbedFooter {Text = $"Requested on {guild.Name} | {guild.Id}", IconUrl = guild.IconUrl}};

                await guild.GetMemberAsync(user.Id)
                           .Result.SendEmbedMessageAsync(embed);
            }

            if (!channel.IsPrivate)
            {
                await channel.DeleteMessageByIdAsync(context.Message.Id);
                logger.Error(e.Exception, $"The command '{commandName}' has been errored by '{user.GetUsertag()}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");
            }
            else
            {
                logger.Error(e.Exception, $"The command '{commandName}' has been errored by '{user.GetUsertag()}' ({user.Id}) in the direct message.");
            }
        }
    }
}