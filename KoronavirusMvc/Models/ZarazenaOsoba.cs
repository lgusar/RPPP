using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class ZarazenaOsoba
    {
        [Display(Name = "Identifikacijski broj", Prompt = "Unesite identifikacijski broj osobe")]
        [Required(ErrorMessage = "Identifikacijski broj je obavezno polje")]
        [MaxLength(15, ErrorMessage = "Identifikacijski broj može sadržavati najviše 15 znakova")]
        public string IdentifikacijskiBroj { get; set; }
        [Display(Name = "Datum zaraze", Prompt = "Unesite datum zaraze")]
        [Required(ErrorMessage = "Datum zaraze je obavezno polje")]
        public DateTime? DatZaraze { get; set; }
        [Display(Name = "Šifra stanja", Prompt = "Unesite šifru stanja")]
        [Required(ErrorMessage = "Šifra stanja je obavezno polje")]
        public int SifraStanja { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public virtual Stanje SifraStanjaNavigation { get; set; }
    }
}
