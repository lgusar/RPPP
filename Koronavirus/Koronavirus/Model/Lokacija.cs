using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class Lokacija
    {
        public Lokacija()
        {
            Statistika = new HashSet<Statistika>();
        }

        public int SifraGrada { get; set; }
        public string SifraDrzave { get; set; }
        public string ImeGrada { get; set; }

        public virtual Drzava SifraDrzaveNavigation { get; set; }
        public virtual ICollection<Statistika> Statistika { get; set; }
    }
}
