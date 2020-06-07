using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Putovanje
    {
        [Display(Name = "Oznaka putovanja", Prompt = "Unesite naziv")]
        [Required(ErrorMessage = "šifra putovanja je obavezno polje")]
        
        public int SifraPutovanja { get; set; }
        [Display(Name = "Identifikacija osobe", Prompt = "Unesite identifikacijski kod osobe")]
        [Required(ErrorMessage = "Identifikacijski broj je obavezno polje")]
        [MaxLength(15, ErrorMessage = "Identifikacijski broj sadrzi maksimalno 15 slova")]
        public string IdentifikacijskiBroj { get; set; }
        [Display(Name = "Datum polaska", Prompt = "Unesite datum")]
        [Required(ErrorMessage = "Datum polaska je obavezno polje")]
        public DateTime DatumPolaska { get; set; }
        [Display(Name = "Datum povratka", Prompt = "Unesite datum")]
        [Required(ErrorMessage = "Datum povratka je obavezno polje")]
        public DateTime DatumVracanja { get; set; }

        public virtual Osoba IdentifikacijskiBrojNavigation { get; set; }
        public List<int> Lokacije { get; set; }
    }
}
