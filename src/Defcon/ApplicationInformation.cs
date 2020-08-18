using System.Reflection;

namespace Defcon
{
    public static class ApplicationInformation
    {
        public static string Version => Assembly.GetExecutingAssembly()
                                                .GetName()
                                                .Version.ToString(3);

        public static string DefaultPrefix => "k!";

        public static string GitHub => "https://github.com/AMPDevelopment/Kaida";

        public static string DiscordServer => "https://discord.gg/WgUDVAk";
    }
}