using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Stozer
    {
        public Stozer()
        {
            Preporuka = new HashSet<Preporuka>();
            Sastanak = new HashSet<Sastanak>();
            StozerOsoba = new HashSet<StozerOsoba>();
        }

        [Required(ErrorMessage = "Šifra stožera je obvezno polje")]
        [Display(Name = "Šifra stožera", Prompt = "Unesite šifru")]
        [RegularExpression("[0-9]+", ErrorMessage = "Šifra stožera mora biti broj")]
        public int SifraStozera { get; set; }

        [Required(ErrorMessage = "Naziv stožera je obvezno polje")]
        [Display(Name = "Naziv stožera", Prompt = "Unesite naziv")]
        [MaxLength(255, ErrorMessage = "Naziv stožera može sadržavati maksimalno 255 znakova")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Predsjednik je obvezno polje")]
        [Display(Name = "Predsjednik", Prompt = "Unesite predsjednika")]
        public string IdPredsjednika { get; set; }

        public virtual Osoba IdPredsjednikaNavigation { get; set; }
        public virtual ICollection<Preporuka> Preporuka { get; set; }
        public virtual ICollection<Sastanak> Sastanak { get; set; }
        public virtual ICollection<StozerOsoba> StozerOsoba { get; set; }
    }
}
