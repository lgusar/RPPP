using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Preporuka
    {
        public Preporuka()
        {
            InverseSifraPrethodnePreporukeNavigation = new HashSet<Preporuka>();
        }

        [Display(Name ="Šifra preporuke", Prompt ="Unesite šifru preporuke")]
        [Required(ErrorMessage ="Šifra preporuke je obavezno polje")]

        public int SifraPreporuke { get; set; }
        
        [Display(Name = "Šifra organizacije", Prompt = "Unesite šifru organizacije")]
        
        public int? SifraOrganizacije { get; set; }

        [Display(Name = "Šifra stožera", Prompt = "Unesite šifru stožera")]
        
        public int? SifraStozera { get; set; }
        [Display(Name = "Šifra prethodne preporuke", Prompt = "Unesite šifru prethodne preporuke")]
        
        public int? SifraPrethodnePreporuke { get; set; }
        [Display(Name = "Opis", Prompt = "Unesite opis preporuke")]
        [Required(ErrorMessage = "Opis preporuke je obavezno polje")]

        public string Opis { get; set; }
        [Display(Name = "Vrijeme objave", Prompt = "Unesite vrijeme objave")]
        [Required(ErrorMessage ="Vrijeme objave preporuke je obavezno polje")]
        public DateTime VrijemeObjave { get; set; }

        public virtual Organizacija SifraOrganizacijeNavigation { get; set; }
        public virtual Preporuka SifraPrethodnePreporukeNavigation { get; set; }
        public virtual Stozer SifraStozeraNavigation { get; set; }
        public virtual ICollection<Preporuka> InverseSifraPrethodnePreporukeNavigation { get; set; }
    }
}
