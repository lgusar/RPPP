using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Terapija
    {
        public Terapija()
        {
            PregledTerapija = new HashSet<PregledTerapija>();
        }

        public int SifraTerapije { get; set; }
        public string OpisTerapije { get; set; }

        public virtual ICollection<PregledTerapija> PregledTerapija { get; set; }
    }
}
