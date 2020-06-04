using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Simptom
    {
        public Simptom()
        {
            PregledSimptom = new HashSet<PregledSimptom>();
        }

        [Display(Name = "Šifra simptoma", Prompt = "Unesite šifru simptoma")]
        [Required(ErrorMessage = "Šifra simptoma je obvezno polje")]
        public int SifraSimptoma { get; set; }

        [Display(Name = "Opis simptoma", Prompt = "Unesite opis simptoma")]
        [Required(ErrorMessage = "Opis simptoma je obvezno polje")]
        public string Opis { get; set; }

        public virtual ICollection<PregledSimptom> PregledSimptom { get; set; }
    }
}
