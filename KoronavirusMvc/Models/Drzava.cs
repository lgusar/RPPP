using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Drzava
    {
        public Drzava()
        {
            Lokacija = new HashSet<Lokacija>();
        }

        [Display(Name = "Oznaka države", Prompt = "Unesite oznaku")]
        [Required(ErrorMessage = "šifra države je obavezno polje")]
        [MaxLength(10, ErrorMessage = "Oznaka putovanja sadrzi maksimalno 10 slova")]
        public string SifraDrzave { get; set; }
        [Display(Name = "Ime države", Prompt = "Unesite naziv")]
        [Required(ErrorMessage = "Ime države je obavezno polje")]
        [MaxLength(80, ErrorMessage = "ime države sadrzi maksimalno 80 slova")]
        public string ImeDrzave { get; set; }

        public virtual ICollection<Lokacija> Lokacija { get; set; }
    }
}
