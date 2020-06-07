using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Mjera
    {
        public Mjera()
        {
            InverseSifraPrethodneMjereNavigation = new HashSet<Mjera>();
        }

        [Required(ErrorMessage = "Šifra mjere je obvezno polje")]
        [Display(Name = "Šifra mjere", Prompt = "Unesite šifru")]
        [RegularExpression("[0-9]+", ErrorMessage = "Šifra mjere mora biti broj")]
        public int SifraMjere { get; set; }

        [Required(ErrorMessage = "Sifra sastanka je obvezno polje")]
        [Display(Name = "Sifra sastanka", Prompt = "Unesite sastanak")]
        public int SifraSastanka { get; set; }

        [Required(ErrorMessage = "Sifra prethodne mjere je obvezno polje")]
        [Display(Name = "Sifra prethodne mjere", Prompt = "Unesite prethodnu mjeru")]
        public int? SifraPrethodneMjere { get; set; }

        [Display(Name = "Opis mjere", Prompt = "Unesite opis mjere")]
        [Required(ErrorMessage = "Opis mjere je obavezno polje")]
        [MaxLength(255, ErrorMessage = "Opis mjere može sadržavati najviše 15 znakova")]
        public string Opis { get; set; }

        [Display(Name = "Datum mjere")]
        [Required(ErrorMessage = "Datum mjere je obavezno polje")]
        public DateTime Datum { get; set; }

        [Display(Name = "Vrijedi do")]
        [Required(ErrorMessage = "Rok trajanja je obavezno polje")]
        public DateTime VrijediDo { get; set; }


        public virtual Mjera SifraPrethodneMjereNavigation { get; set; }
        public virtual Sastanak SifraSastankaNavigation { get; set; }
        public virtual ICollection<Mjera> InverseSifraPrethodneMjereNavigation { get; set; }
    }
}
