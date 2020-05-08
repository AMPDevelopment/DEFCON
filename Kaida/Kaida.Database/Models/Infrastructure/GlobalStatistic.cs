using System;
using System.Collections.Generic;
using System.Text;

namespace Kaida.Database.Models.Infrastructure
{
    public class GlobalStatistic : Statistic
    {
        public int Guilds { get; set; }
        public int Users { get; set; }
        public int Shards { get; set; }
    }
}