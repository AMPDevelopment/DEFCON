namespace Defcon.Library.Redis
{
    public static class RedisKeyNaming
    {
        public static string Config => "config";

        public static string Guild(ulong guildId) => $"Guilds:{guildId}";

        public static string User(ulong userId) => $"Users:{userId}";
    }
}