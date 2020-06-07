using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class OsobaPregled
    {
        [Display(Name = "Identifikacijski broj osobe", Prompt = "Unesite identifikacijski broj osobe")]
        [Required(ErrorMessage = "Identifikacijski broj osobe je obvezno polje")]
        public string IdentifikacijskiBroj { get; set; }
        public int? SifraPregleda { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public virtual Pregled SifraPregledaNavigation { get; set; }
    }
}
