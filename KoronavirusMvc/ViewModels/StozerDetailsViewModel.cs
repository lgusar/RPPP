using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class StozerDetailsViewModel
    {
        public int SifraStozera { get; set; }
        public string Naziv { get; set; }
        public string ImePredsjednika { get; set; }

        public virtual Osoba IdPredsjednikaNavigation { get; set; }
        public virtual ICollection<Preporuka> Preporuka { get; set; }
        public virtual ICollection<StozerOsoba> StozerOsoba { get; set; }

        public Sastanak Sastanak;
        public List<SastanakViewModel> Sastanci;
        public PagingInfo PagingInfo;
        public ZarazenaOsoba ZarazenaOsoba;
        public ZarazenaOsobaViewModel ZarazeneOsobe;

    }
}
