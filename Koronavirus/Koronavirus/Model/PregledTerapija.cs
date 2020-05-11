using System;
using System.Collections.Generic;

namespace Koronavirus.Model
{
    public partial class PregledTerapija
    {
        public int SifraPregleda { get; set; }
        public int SifraTerapije { get; set; }

        public virtual Pregled SifraPregledaNavigation { get; set; }
        public virtual Terapija SifraTerapijeNavigation { get; set; }
    }
}
