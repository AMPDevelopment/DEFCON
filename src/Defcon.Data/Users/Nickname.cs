﻿using System;

namespace Defcon.Data.Users
{
    public class Nickname
    {
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset DateTime { get; set; }
    }
}