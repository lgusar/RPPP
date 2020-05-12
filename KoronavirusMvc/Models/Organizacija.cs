using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Organizacija
    {
        public Organizacija()
        {
            Institucija = new HashSet<Institucija>();
            Preporuka = new HashSet<Preporuka>();
            Statistika = new HashSet<Statistika>();
        }

        public int SifraOrganizacije { get; set; }
        public string Naziv { get; set; }
        public string Url { get; set; }

        public virtual ICollection<Institucija> Institucija { get; set; }
        public virtual ICollection<Preporuka> Preporuka { get; set; }
        public virtual ICollection<Statistika> Statistika { get; set; }
    }
}
