using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Institucija
    {
        public Institucija()
        {
            Oprema = new HashSet<Oprema>();
        }

        [Display(Name = "Šifra institucije", Prompt = "Unesite šifru institucije")]
        [Required(ErrorMessage = "Šifra institucije je obavezno polje")]
        public int SifraInstitucije { get; set; }

        [Display(Name = "Naziv institucije", Prompt = "Unesite naziv institucije")]
        [Required(ErrorMessage = "Naziv institucije je obavezno polje")]
        public string NazivInstitucije { get; set; }

        [Display(Name = "Radno vrijeme", Prompt = "Unesite radno vrijeme institucije")]
        public TimeSpan RadnoVrijeme { get; set; }

        [Display(Name = "Kontakt institucije", Prompt = "Unesite kontakt institucije")]
        public string Kontakt { get; set; }
        
        [Display(Name = "Šifra organizacije", Prompt = "Unesite šifru organizacije")]
        public int? SifraOrganizacije { get; set; }

        public virtual Organizacija SifraOrganizacijeNavigation { get; set; }
        public virtual ICollection<Oprema> Oprema { get; set; }
    }
}
