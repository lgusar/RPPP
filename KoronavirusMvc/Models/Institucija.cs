using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Institucija
    {
        public Institucija()
        {
            Oprema = new HashSet<Oprema>();
        }

        public int SifraInstitucije { get; set; }
        public string NazivInstitucije { get; set; }
        public TimeSpan RadnoVrijeme { get; set; }
        public string Kontakt { get; set; }
        public int? SifraOrganizacije { get; set; }

        public virtual Organizacija SifraOrganizacijeNavigation { get; set; }
        public virtual ICollection<Oprema> Oprema { get; set; }
    }
}
