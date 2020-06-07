using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KoronavirusMvc.Models
{
    public partial class Institucija
    {
        public Institucija()
        {
            Oprema = new HashSet<Oprema>();
        }

        public int SifraInstitucije { get; set; }

        [Display(Name = "Naziv institucije", Prompt = "Unesite naziv institucije")]
        [Required(ErrorMessage = "Naziv institucije je obavezno polje")]
        public string NazivInstitucije { get; set; }

        [Display(Name = "Radno vrijeme", Prompt = "Unesite radno vrijeme institucije")]
        [RegularExpression("([0-9]|1[0-9]|2[0-4]):[0-5][0-9]-([0-9]|1[0-9]|2[0-4]):[0-5][0-9]", ErrorMessage ="Radno vrijeme mora biti u obliku hh:mm-hh:mm")]
        public string RadnoVrijeme { get; set; }

        [Display(Name = "Kontakt institucije", Prompt = "Unesite kontakt institucije")]
        [RegularExpression("[0-9]{7,11}")]
        public string Kontakt { get; set; }

        [Display(Name = "Šifra organizacije", Prompt = "Unesite šifru organizacije")]
        public int SifraOrganizacije { get; set; }

        public virtual Organizacija SifraOrganizacijeNavigation { get; set; }
        public virtual ICollection<Oprema> Oprema { get; set; }
    }
}
