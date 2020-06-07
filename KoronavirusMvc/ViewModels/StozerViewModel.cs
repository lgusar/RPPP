using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class StozerViewModel
    {
        public int SifraStozera { get; set; }

        public string Naziv { get; set; }
        public string ImePredsjednika { get; set; }

        public virtual Osoba IdPredsjednikaNavigation { get; set; }
        public virtual ICollection<Preporuka> Preporuka { get; set; }
        public virtual ICollection<Sastanak> Sastanak { get; set; }
        public virtual ICollection<StozerOsoba> StozerOsoba { get; set; }
    }
}