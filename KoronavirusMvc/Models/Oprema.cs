using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Oprema
    {

        public int SifraOpreme { get; set; }

        [Display(Name = "Šifra institucije", Prompt = "Odaberite instituciju")]
        [Required(ErrorMessage = "Institucija je obavezno polje")]
        public int SifraInstitucije { get; set; }

        [Display(Name = "Naziv opreme", Prompt = "Unesite naziv opreme")]
        [Required(ErrorMessage = "Naziv opreme je obavezno polje")]
        public string NazivOpreme { get; set; }

        [Display(Name = "Količina opreme", Prompt = "Unesite količinu opreme")]
        [Required(ErrorMessage = "Količina opreme je obavezno polje")]
        [Range(1, 10000, ErrorMessage = "Oprema mora biti od 1 do 10000")]
        public int KolicinaOpreme { get; set; }

        public virtual Institucija SifraInstitucijeNavigation { get; set; }
    }
}
