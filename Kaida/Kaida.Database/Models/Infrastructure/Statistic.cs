using System;
using System.Collections.Generic;
using System.Text;

namespace Kaida.Database.Models.Infrastructure
{
    public class Statistic
    {
        public int Commands { get; set; }
        public int Warns { get; set; }
        public int Muted { get; set; }
        public int Bans { get; set; }
    }
}