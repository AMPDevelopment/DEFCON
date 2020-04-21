using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Entities.Discord;
using Kaida.Library.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        public async Task PingPong(CommandContext context)
        {
            var fields = new List<EmbedField>();

            fields.Add(new EmbedField
            {
                Inline = true,
                Name = "Ping",
                Value = $":ping_pong: {context.Client.Ping}ms"
            });

            await context.EmbeddedMessage(title: "Client Status", fields: fields);
        }
    }
}
