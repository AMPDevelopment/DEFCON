using System.Collections.Generic;
using Kaida.Data.Roles;

namespace Kaida.Data.Guilds
{
    public class Guild
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public IList<ulong> ModeratorRoleIds { get; set; }
        public bool ModeratorAllowedWarn { get; set; }
        public bool ModeratorAllowedMute { get; set; }
        public bool ModeratorAllowedKick { get; set; }
        public bool ModeratorAllowedBan { get; set; }
        public IList<Log> Logs { get; set; }
        public IList<Setting> Settings { get; set; }
        public IList<ReactionMenu> ReactionMenus { get; set; }
        public IList<ReactionSingle> ReactionSingles { get; set; }
    }
}