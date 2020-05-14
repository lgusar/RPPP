using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class PregledTerapija
    {
        public int SifraPregleda { get; set; }
        public int SifraTerapije { get; set; }

        public virtual Pregled SifraPregledaNavigation { get; set; }
        public virtual Terapija SifraTerapijeNavigation { get; set; }
    }
}
