using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class StozerOsoba
    {
        public string IdentifikacijskiBroj { get; set; }
        public int SifraStozera { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public virtual Stozer SifraStozeraNavigation { get; set; }
    }
}
