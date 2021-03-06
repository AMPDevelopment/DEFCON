﻿using System.Collections.Generic;
using Defcon.Data.Roles;

namespace Defcon.Data.Guilds
{
    public class Guild
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public IList<ulong> ModeratorRoleIds { get; set; }
        public bool AllowWarnModerators { get; set; }
        public bool AllowMuteModerators { get; set; }
        public IList<Log> Logs { get; set; }
        public IList<Setting> Settings { get; set; }
        public IList<ReactionMessage> ReactionMessages { get; set; }
        public IList<ReactionCategory> ReactionCategories { get; set; }
    }
}