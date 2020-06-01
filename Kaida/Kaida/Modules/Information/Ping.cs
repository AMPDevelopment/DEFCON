using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Attributes;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Modules.Information
{
    [Category("Information")]
    [Group("Ping")]
    public class Ping : BaseCommandModule
    {
        private readonly ILogger logger;

        public Ping(ILogger logger)
        {
            this.logger = logger;
        }

        [GroupCommand]
        public async Task Pong(CommandContext context)
        {
            var fields = new List<EmbedField> {new EmbedField
            {
                Inline = true, 
                Name = "Ping",
                Value = $":KaidaPing: {context.Client.Ping}ms"
            }};
            var embed = new Embed {Title = "Status", Fields = fields};

            await context.SendEmbedMessageAsync(embed);
        }
    }
}