using System.Collections.Generic;

namespace Kaida.Data.Roles
{
    public class ReactionSingle
    {
        public ulong Id { get; set; }
        public List<ReactionItem> ReactionItems { get; set; }
    }
}