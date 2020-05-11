using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class PregledSimptom
    {
        public int SifraPregleda { get; set; }
        public int SifraSimptoma { get; set; }

        public virtual Pregled SifraPregledaNavigation { get; set; }
        public virtual Simptom SifraSimptomaNavigation { get; set; }
    }
}
