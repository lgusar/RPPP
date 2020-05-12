using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class ZarazenaOsoba
    {
        public string IdentifikacijskiBroj { get; set; }
        public DateTime? DatZaraze { get; set; }
        public int SifraStanja { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public virtual Stanje SifraStanjaNavigation { get; set; }
    }
}
