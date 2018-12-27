using Serilog;

namespace Kaida.Library.Logger
{
    public class LoggerFactory
    {
        public static ILogger CreateLogger()
        {
            var outputTemplate = "[{Timestamp:dd-MM-yyyy HH:mm}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.File("kaida.log");

            return logger.CreateLogger();
        }
    }
}
