using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Stozer
    {
        public Stozer()
        {
            Preporuka = new HashSet<Preporuka>();
            Sastanak = new HashSet<Sastanak>();
            StozerOsoba = new HashSet<StozerOsoba>();
        }

        public int SifraStozera { get; set; }
        public string Naziv { get; set; }
        public string IdPredsjednika { get; set; }

        public virtual Osoba IdPredsjednikaNavigation { get; set; }
        public virtual ICollection<Preporuka> Preporuka { get; set; }
        public virtual ICollection<Sastanak> Sastanak { get; set; }
        public virtual ICollection<StozerOsoba> StozerOsoba { get; set; }
    }
}
