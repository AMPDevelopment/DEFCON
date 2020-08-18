using System.Collections.Generic;

namespace Defcon.Data.Roles
{
    public abstract class ReactionBase
    {
        public ulong Id { get; set; }
        public List<ReactionRole> ReactionRoles { get; set; }
    }
}
