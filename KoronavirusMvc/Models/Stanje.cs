using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
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
