using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Pregled
    {
        public Pregled()
        {
            PregledSimptom = new HashSet<PregledSimptom>();
            PregledTerapija = new HashSet<PregledTerapija>();
        }

        public int SifraPregleda { get; set; }
        public DateTime Datum { get; set; }
        public string Anamneza { get; set; }
        public string Dijagnoza { get; set; }

        public virtual ICollection<PregledSimptom> PregledSimptom { get; set; }
        public virtual ICollection<PregledTerapija> PregledTerapija { get; set; }
    }
}
