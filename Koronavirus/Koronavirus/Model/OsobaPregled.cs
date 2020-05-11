using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class OsobaPregled
    {
        public string IdentifikacijskiBroj { get; set; }
        public int? SifraPregleda { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public virtual Pregled SifraPregledaNavigation { get; set; }
    }
}
