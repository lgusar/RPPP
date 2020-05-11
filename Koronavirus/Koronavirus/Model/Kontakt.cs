using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class Kontakt
    {
        public string IdOsoba { get; set; }
        public string IdKontakt { get; set; }

        public virtual Osoba IdKontaktNavigation { get; set; }
        public virtual Osoba IdOsobaNavigation { get; set; }
    }
}
