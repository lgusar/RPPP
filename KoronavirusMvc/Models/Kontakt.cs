using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Kontakt
    {
        [Display(Name = "Identifikacijski broj osobe", Prompt = "Unesite identifikacijski broj osobe")]
        [Required(ErrorMessage = "Identifikacijski broj osobe je obavezno polje")]
        [MaxLength(15, ErrorMessage = "Identifikacijski broj može sadržavati najviše 15 znakova")]
        public string IdOsoba { get; set; }
        [Display(Name = "Identifikacijski broj osobe s kojom je bila u kontaktu", Prompt = "Unesite identifikacijski broj osobe s kojom je bila u kontaktu")]
        [Required(ErrorMessage = "Identifikacijski broj je obavezno polje")]
        [MaxLength(15, ErrorMessage = "Identifikacijski broj može sadržavati najviše 15 znakova")]
        public string IdKontakt { get; set; }

        public virtual Osoba IdKontaktNavigation { get; set; }
        public virtual Osoba IdOsobaNavigation { get; set; }
    }
}
