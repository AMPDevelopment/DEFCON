using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Defcon.Core;
using Defcon.Library.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using MoreLinq;

namespace Defcon.Library.Formatters
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private readonly DiscordEmbedBuilder embed;
        private Command Command { get; set; }
        private new CommandContext Context { get; set; }
        private string Title { get; set; }

        public HelpFormatter(CommandContext context)
            : base(context)
        {
            Context = context;
            embed = new DiscordEmbedBuilder().WithTitle("Command list")
                                             .WithColor(DiscordColor.Turquoise)
                                             .AddRequestedByFooter(context.User);
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: embed);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            Command = command;
            embed.WithTitle($"Command [{Command.QualifiedName}]");
            embed.Description = $"{Formatter.InlineCode(command.Name)}: {command.Description ?? "No description available."}";

            if (command is CommandGroup commandGroup && commandGroup.IsExecutableWithoutSubcommands)
            {
                embed.Description = $"{embed.Description}\n\nThis group can be executed as a standalone command.";
            }

            if (command.Aliases?.Any() == true)
            {
                embed.AddField("Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)), false);
            }

            if (command.Overloads?.Any() == true)
            {
                var arguments = new StringBuilder();

                foreach (var overload in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    arguments.Append('`').Append(command.QualifiedName);

                    foreach (var arg in overload.Arguments)
                        arguments.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

                    arguments.Append("`\n");

                    foreach (var arg in overload.Arguments)
                    {
                        arguments.Append('`').Append(arg.Name).Append(" (").Append(CommandsNext.GetUserFriendlyTypeName(arg.Type)).Append(")`: ").Append(arg.Description ?? "No description provided.").Append('\n');
                    }
                        

                    arguments.Append('\n');
                }

                embed.AddField("Arguments", arguments.ToString().Trim(), false);
            }

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            var categories = subcommands.Where(x => x.Name != "help" && x.IsHidden == false).Select(c => c.Category()).DistinctBy(x => x);

            foreach (var category in categories)
            {
                embed.AddField(Command != null ? "Subcommands" : category, string.Join(", ", subcommands.Where(c => c.Category() == category).Select(x => Formatter.InlineCode(x.Name))), false);
            }

            if (Command == null)
            {
                embed.AddField("For more information type", $"help <command>", false);
                embed.AddField("Support Server", Formatter.MaskedUrl("Join our support server", new Uri(ApplicationInformation.DiscordServer)), true);
                embed.AddField("Add DEFCON", Formatter.MaskedUrl("Invite DEFCON to your server", new Uri(Context.Client.GenerateInviteLink())), true);
            }
            return this;
        }
    }
}