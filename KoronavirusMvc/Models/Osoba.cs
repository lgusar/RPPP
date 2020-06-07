using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Osoba
    {
        public Osoba()
        {
            KontaktIdKontaktNavigation = new HashSet<Kontakt>();
        }
        
        [Display(Name = "Identifikacijski broj", Prompt="Unesite identifikacijski broj osobe")]
        [Required(ErrorMessage ="Identifikacijski broj je obavezno polje")]
        [MaxLength(15, ErrorMessage ="Identifikacijski broj može sadržavati najviše 15 znakova")]
        public string IdentifikacijskiBroj { get; set; }
        [Display(Name = "Ime", Prompt = "Unesite ime osobe")]
        [Required(ErrorMessage = "Ime je obavezno polje")]
        public string Ime { get; set; }
        [Display(Name = "Prezime", Prompt = "Unesite prezime osobe")]
        [Required(ErrorMessage = "Prezime je obavezno polje")]
        public string Prezime { get; set; }
        [Display(Name = "Adresa stanovanja", Prompt = "Unesite adresu stanovanja")]
        [Required(ErrorMessage = "Adresa je obavezno polje")]
        public string Adresa { get; set; }
        [Display(Name = "Datum rođenja")]
        [Required(ErrorMessage = "Datum rođenja je obavezno polje")]
        public DateTime DatRod { get; set; }
        [Display(Name = "Zanimanje", Prompt = "Unesite zanimanje osobe")]
        public string Zanimanje { get; set; }

        public virtual Putovanje Putovanje { get; set; }
        public virtual StozerOsoba StozerOsoba { get; set; }
        public virtual ZarazenaOsoba ZarazenaOsoba { get; set; }
        public virtual ICollection<Kontakt> KontaktIdKontaktNavigation { get; set; }
        public virtual ICollection<Kontakt> KontaktIdOsobaNavigation { get; set; }
        public virtual ICollection<Stozer> Stozer { get; set; }

        internal static object imePrezime()
        {
            throw new NotImplementedException();
        }
    }
}
