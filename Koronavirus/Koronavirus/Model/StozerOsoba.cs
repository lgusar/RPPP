using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class StozerOsoba
    {
        public string IdentifikacijskiBroj { get; set; }
        public int SifraStozera { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public virtual Stozer SifraStozeraNavigation { get; set; }
    }
}
