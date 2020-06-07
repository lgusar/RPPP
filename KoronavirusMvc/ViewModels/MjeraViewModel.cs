using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class MjeraViewModel
    {
        public int SifraMjere { get; set; }
        public int SifraSastanka { get; set; }
        public int? SifraPrethodneMjere { get; set; }
        public string Opis { get; set; }
        public DateTime Datum { get; set; }
        public DateTime VrijediDo { get; set; }

        public virtual Mjera SifraPrethodneMjereNavigation { get; set; }
        public virtual Sastanak SifraSastankaNavigation { get; set; }
        public virtual ICollection<Mjera> InverseSifraPrethodneMjereNavigation { get; set; }

    }
}