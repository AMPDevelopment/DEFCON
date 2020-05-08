using System;
using System.Collections.Generic;
using System.Text;

namespace Kaida.Database.Models.Users
{
    public class Infraction
    {
        public int Id { get; set; }
        public ulong ModeratorId { get; set; }
        public ulong GuildId { get; set; }
        public InfractionType InfractionType { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}