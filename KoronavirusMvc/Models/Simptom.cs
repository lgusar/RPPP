using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Simptom
    {
        public Simptom()
        {
            PregledSimptom = new HashSet<PregledSimptom>();
        }

        public int SifraSimptoma { get; set; }
        public string Opis { get; set; }

        public virtual ICollection<PregledSimptom> PregledSimptom { get; set; }
    }
}
