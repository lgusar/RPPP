using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class OsobaPregled
    {
        public string IdentifikacijskiBroj { get; set; }
        public int? SifraPregleda { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public virtual Pregled SifraPregledaNavigation { get; set; }
    }
}
