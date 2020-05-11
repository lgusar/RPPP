using System;
using System.Collections.Generic;

namespace Koronavirus.Model
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
