using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Oprema
    {
        public int SifraOpreme { get; set; }
        public int SifraInstitucije { get; set; }
        public string NazivOpreme { get; set; }
        public int KolicinaOpreme { get; set; }

        public virtual Institucija SifraInstitucijeNavigation { get; set; }
    }
}
