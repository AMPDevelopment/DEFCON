using Serilog;
using Serilog.Events;

namespace Defcon.Library.Logger
{
    public class LoggerFactory
    {
        public static ILogger CreateLogger()
        {
            const string outputTemplate = "[{Timestamp:dd-MM-yyyy HH:mm}] [{Level:u3}] [DEFCON] {Message:lj}{NewLine}{Exception}";
            var logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: outputTemplate, restrictedToMinimumLevel: LogEventLevel.Verbose)
                                                  .WriteTo.File("defcon.log", outputTemplate: outputTemplate, restrictedToMinimumLevel: LogEventLevel.Information)
                                                  .WriteTo.File("defcon_error.log", outputTemplate: outputTemplate, restrictedToMinimumLevel: LogEventLevel.Error);

            return logger.CreateLogger();
        }
    }
}