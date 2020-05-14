using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Stanje
    {
        public Stanje()
        {
            ZarazenaOsoba = new HashSet<ZarazenaOsoba>();
        }
        [Display(Name = "Šifra stanja", Prompt = "Unesite šifru stanja")]
        [Required(ErrorMessage = "Šifra stanja je obavezno polje")]
        public int SifraStanja { get; set; }
        [Display(Name = "Naziv stanja", Prompt = "Unesite naziv stanja")]
        [Required(ErrorMessage = "Naziv stanja je obavezno polje")]
        public string NazivStanja { get; set; }

        public virtual ICollection<ZarazenaOsoba> ZarazenaOsoba { get; set; }
    }
}
