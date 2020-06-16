using System.Collections.Generic;

namespace Kaida.Data.Roles
{
    public abstract class ReactionBase
    {
        public ulong Id { get; set; }
        public List<ReactionRole> ReactionRoles { get; set; }
    }
}
