using System.Collections.Generic;

namespace Kaida.Data.Roles
{
    public class ReactionMenu
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public List<ReactionItem> ReactionItems { get; set; }
    }
}