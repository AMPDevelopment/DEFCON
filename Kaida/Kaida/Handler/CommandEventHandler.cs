using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Kaida.Library.Extensions;
using Serilog;

namespace Kaida.Handler
{
    public class CommandEventHandler
    {
        private readonly CommandsNextExtension _cnext;
        private readonly ILogger _logger;

        public CommandEventHandler(CommandsNextExtension cnext, ILogger logger)
        {
            _cnext = cnext;
            _logger = logger;

            _cnext.CommandExecuted += CommandExecuted;
            _cnext.CommandErrored += CommandErrored;
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
                    _logger.Information($"The command '{command.Name}' has been executed by '{user.GetUsertag()}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");
                    await channel.DeleteMessageByIdAsync(message.Id);
                }
                else
                {
                    _logger.Information($"The command '{command.Name}' has been executed by an unknown user in a deleted channel on a unknown guild.");
                }
            }
            else
            {
                _logger.Information($"The command '{command.Name}' has been executed by '{user.GetUsertag()}' ({user.Id}) in the direct message.");
            }
        }

        private async Task CommandErrored(CommandErrorEventArgs e)
        {
            var command = e.Command;
            var context = e.Context;
            var guild = context.Guild;
            var channel = context.Channel;
            var user = context.User;

            if (!channel.IsPrivate)
            {
                if (channel != null && user != null)
                {
                    await channel.DeleteMessageByIdAsync(context.Message.Id);
                    _logger.Error(e.Exception, $"The command '{command.Name}' has been errored by '{user.GetUsertag()}' in the channel '{channel.Name}' ({channel.Id}) on the guild '{guild.Name}' ({guild.Id}).");
                }
                else
                {
                    _logger.Error(e.Exception, $"The command '{command.Name}' has been errored by an unknown user in a deleted channel on a unknown guild.");
                }
            }
            else
            {
                _logger.Information($"The command '{command.Name}' has been errored by '{user.GetUsertag()}' ({user.Id}) in the direct message.");
            }
        }
    }
}