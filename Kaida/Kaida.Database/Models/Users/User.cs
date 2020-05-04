using System;
using System.Collections.Generic;
using System.Text;

namespace Kaida.Database.Models.Users
{
    public class User
    {
        public ulong Id { get; set; }
        public IList<Infraction> Infractions { get; set; }
    }
}
