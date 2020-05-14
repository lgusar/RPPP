using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Organizacija
    {
        public Organizacija()
        {
            Institucija = new HashSet<Institucija>();
            Preporuka = new HashSet<Preporuka>();
            Statistika = new HashSet<Statistika>();
        }

        [Display(Name = "Šifra organizacije", Prompt = "Unesite šifru organizacije")]
        [Required(ErrorMessage = "Šifra organizacije je obavezno polje")]
        public int SifraOrganizacije { get; set; }

        [Display(Name = "Naziv organizacije", Prompt = "Unesite naziv organizacije")]
        [Required(ErrorMessage = "Naziv organizacije je obavezno polje")]
        public string Naziv { get; set; }

        [Display(Name = "URL organizacije", Prompt = "Unesite url stranice organizacije")]
        public string Url { get; set; }

        public virtual ICollection<Institucija> Institucija { get; set; }
        public virtual ICollection<Preporuka> Preporuka { get; set; }
        public virtual ICollection<Statistika> Statistika { get; set; }
    }
}
