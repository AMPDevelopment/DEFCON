using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace Kaida.Library.Formatter
{
    public class HelpFormatter : BaseHelpFormatter
    {
        public DiscordEmbedBuilder EmbedBuilder { get; }
        private Command Command { get; set; }

        public HelpFormatter(CommandContext context) : base(context)
        {
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            throw new NotImplementedException();
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            throw new NotImplementedException();
        }

        public override CommandHelpMessage Build()
        {
            if (this.Command == null)
                this.EmbedBuilder.WithDescription("Listing all top-level commands and groups. Specify a command to see more information.");

            return new CommandHelpMessage(embed: this.EmbedBuilder.Build());
        }
    }
}
