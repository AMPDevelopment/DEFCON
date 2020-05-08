using System;
using System.Collections.Generic;
using System.Text;

namespace Kaida.Database.Models.Guilds
{
    public class Log
    {
        public ulong ChannelId { get; set; }
        public LogType LogType { get; set; }
    }
}