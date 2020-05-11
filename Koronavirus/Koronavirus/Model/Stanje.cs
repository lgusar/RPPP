using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class Stanje
    {
        public Stanje()
        {
            ZarazenaOsoba = new HashSet<ZarazenaOsoba>();
        }

        public int SifraStanja { get; set; }
        public string NazivStanja { get; set; }

        public virtual ICollection<ZarazenaOsoba> ZarazenaOsoba { get; set; }
    }
}
