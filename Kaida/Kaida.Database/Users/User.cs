using System;
using System.Collections.Generic;

namespace Kaida.Data.Users
{
    public class User
    {
        public ulong Id { get; set; }
        public DateTimeOffset? Birthdate { get; set; }
        public string Description { get; set; }
        public ulong? SteamId { get; set; }
        public IList<Nickname> Nicknames { get; set; }
        public IList<Infraction> Infractions { get; set; }
        public int InfractionId { get; set; }
    }
}