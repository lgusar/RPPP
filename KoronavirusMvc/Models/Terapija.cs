using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Terapija
    {
        public Terapija()
        {
            PregledTerapija = new HashSet<PregledTerapija>();
        }

        [Display(Name = "Šifra terapije", Prompt = "Unesite šifru terapije")]
        [Required(ErrorMessage = "Šifra terapije je obvezno polje")]
        public int SifraTerapije { get; set; }

        [Display(Name = "Opis terapije", Prompt = "Unesite terapiju")]
        [Required(ErrorMessage = "Opis terapije je obvezno polje")]
        public string OpisTerapije { get; set; }

        public virtual ICollection<PregledTerapija> PregledTerapija { get; set; }
    }
}
