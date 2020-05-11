using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Koronavirus.Model
{
    public partial class Osoba
    {
        public Osoba()
        {
            KontaktIdKontaktNavigation = new HashSet<Kontakt>();
            KontaktIdOsobaNavigation = new HashSet<Kontakt>();
            Stozer = new HashSet<Stozer>();
        }

        [Required(ErrorMessage ="Obavezno upisati identifikacijski broj")]
        public string IdentifikacijskiBroj { get; set; }
        [Required(ErrorMessage ="Ime je obavezno polje")]
        [RegularExpression(@"^[A-Za-z-čćžšđ ]*$", ErrorMessage =("Ime nije valjano"))]
        public string Ime { get; set; }
        [Required(ErrorMessage = "Prezime je obavezno polje")]
        [RegularExpression(@"^[A-Za-z-čćžšđ ]*$", ErrorMessage = ("Prezime nije valjano"))]
        public string Prezime { get; set; }
        public string Adresa { get; set; }
        public DateTime DatRod { get; set; }
        public string Zanimanje { get; set; }

        public virtual Putovanje Putovanje { get; set; }
        public virtual StozerOsoba StozerOsoba { get; set; }
        public virtual ZarazenaOsoba ZarazenaOsoba { get; set; }
        public virtual ICollection<Kontakt> KontaktIdKontaktNavigation { get; set; }
        public virtual ICollection<Kontakt> KontaktIdOsobaNavigation { get; set; }
        public virtual ICollection<Stozer> Stozer { get; set; }
    }
}
