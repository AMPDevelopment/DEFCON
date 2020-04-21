using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Handler
{
    public class CommandEventHandler
    {
        private readonly ILogger _logger;
        private readonly CommandsNextExtension _cnext;

        public CommandEventHandler(CommandsNextExtension cnext, ILogger logger)
        {
            _cnext = cnext;
            _logger = logger;

            _cnext.CommandExecuted += CommandExecuted;
            _cnext.CommandErrored += CommandErrored;
        }

        private async Task CommandExecuted(CommandExecutionEventArgs e)
        {
            
            if (e.Context.Channel != null) 
            {
                _logger.Information($"The command '{e.Command.Name}' has been executed by '{e.Context.User.Username}#{e.Context.User.Discriminator}' in the channel '{e.Context.Channel.Name}' ({e.Context.Channel.Id}) on the guild '{e.Context.Guild.Name}' ({e.Context.Guild.Id}).");
                await e.Context.Channel.DeleteMessageByIdAsync(e.Context.Message.Id); 
            }
            else
            {
                _logger.Information($"The command '{e.Command.Name}' has been executed by an unknown user in a deleted channel on a unknown guild.");
            }
        }

        private async Task CommandErrored(CommandErrorEventArgs e)
        {
            
            if (e.Context.Channel != null)
            {
                await e.Context.Channel.DeleteMessageByIdAsync(e.Context.Message.Id);
                _logger.Error(e.Exception, $"The command '{e.Command.Name}' has been errored by '{e.Context.User.Username}#{e.Context.User.Discriminator}' in the channel '{e.Context.Channel.Name}' ({e.Context.Channel.Id}) on the guild '{e.Context.Guild.Name}' ({e.Context.Guild.Id}).");
            }
            else
            {
                _logger.Error(e.Exception, $"The command '{e.Command.Name}' has been errored by an unknown user in a deleted channel on a unknown guild.");
            }
        }
    }
}
