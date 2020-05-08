using System;
using System.Collections.Generic;
using System.Text;

namespace Kaida.Database.Models.Guilds
{
    public class Guild
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public IList<ulong> ModeratorIds { get; set; }
        public bool ModeratorAllowedWarn { get; set; }
        public bool ModeratorAllowedMute { get; set; }
        public bool ModeratorAllowedBan { get; set; }
        public IList<Log> Logs { get; set; }
    }
}