using System.Threading.Tasks;
using Defcon.Core.Database;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;

namespace Defcon.Modules.Owner
{
    [RequireOwner]
    [Hidden]
    public class DatabaseTest : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly MySql mySql;

        public DatabaseTest(ILogger logger, MySql mySql)
        {
            this.logger = logger;
            this.mySql = mySql;
        }

        /// <summary>
        /// Test command
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [Command("db")]
        public async Task Name(CommandContext context)
        {
            using (mySql)
            {
                await context.Channel.SendMessageAsync(mySql.Connection.Database.ToString());
            }
        }
    }
}