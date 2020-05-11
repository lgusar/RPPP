using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class Preporuka
    {
        public Preporuka()
        {
            InverseSifraPrethodnePreporukeNavigation = new HashSet<Preporuka>();
        }

        public int SifraPreporuke { get; set; }
        public int? SifraOrganizacije { get; set; }
        public int? SifraStozera { get; set; }
        public int? SifraPrethodnePreporuke { get; set; }
        public string Opis { get; set; }
        public DateTime VrijemeObjave { get; set; }

        public virtual Organizacija SifraOrganizacijeNavigation { get; set; }
        public virtual Preporuka SifraPrethodnePreporukeNavigation { get; set; }
        public virtual Stozer SifraStozeraNavigation { get; set; }
        public virtual ICollection<Preporuka> InverseSifraPrethodnePreporukeNavigation { get; set; }
    }
}
