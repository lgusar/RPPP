using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class Putovanje
    {
        public int SifraPutovanja { get; set; }
        public string IdentifikacijskiBroj { get; set; }
        public DateTime DatumPolaska { get; set; }
        public DateTime DatumVracanja { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
    }
}
