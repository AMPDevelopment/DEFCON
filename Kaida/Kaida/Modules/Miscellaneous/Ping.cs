using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Entities.Discord;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules.Miscellaneous
{
    public class Ping : BaseCommandModule
    {
        private readonly ILogger _logger;

        public Ping(ILogger logger)
        {
            _logger = logger;
        }

        [Command("Ping")]
        public async Task Pong(CommandContext context)
        {
            var fields = new List<EmbedField>
            {
                new EmbedField {Inline = true, Name = "Ping", Value = $":ping_pong: {context.Client.Ping}ms"}
            };

            await context.EmbeddedMessage("Client Status", fields: fields);
        }
    }
}