using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Sastanak
    {
        public Sastanak()
        {
            Mjera = new HashSet<Mjera>();
        }

        [Required(ErrorMessage = "Šifra sastanka je obvezno polje")]
        [Display(Name = "Šifra sastanka", Prompt = "Unesite šifru")]
        [RegularExpression("[0-9]+", ErrorMessage = "Šifra sastanka mora biti broj")]
        public int SifraSastanka { get; set; }
        
        public int SifraStozera { get; set; }

        [Required(ErrorMessage = "Datum je obvezno polje")]
        [Display(Name = "Datum", Prompt = "Unesite datum")]
        [DataType(DataType.DateTime, ErrorMessage = "Datum nije dobrog formata")]
        public DateTime Datum { get; set; }

        public virtual Stozer SifraStozeraNavigation { get; set; }
        public virtual ICollection<Mjera> Mjera { get; set; }
    }
}
