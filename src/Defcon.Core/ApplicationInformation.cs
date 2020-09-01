using System.Reflection;

namespace Defcon.Core
{
    public static class ApplicationInformation
    {
        public static string Version => Assembly.GetExecutingAssembly()
                                                .GetName()
                                                .Version.ToString(3);

        public static string DefaultPrefix => "d!";
        
        public static string GitHub => "https://github.com/AMPDevelopment/DEFCON";
        
        public static string DiscordServer => "https://discord.gg/WgUDVAk";
    }
}