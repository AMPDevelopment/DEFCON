using System;

namespace Kaida.Data.Reports
{
    public class Report
    {
        public Guid Guid { get; set; }
        public ulong SuspectId { get; set; }
        public ulong VictimId { get; set; }
        public ulong GuildId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}