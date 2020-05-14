using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Pregled
    {
        public Pregled()
        {
            PregledSimptom = new HashSet<PregledSimptom>();
            PregledTerapija = new HashSet<PregledTerapija>();
        }
        
        [Display(Name = "Šifra pregleda", Prompt = "Unesite šifru pregleda")]
        [Required(ErrorMessage = "Šifra pregleda je obvezno polje")]
        public int SifraPregleda { get; set; }

        [Display(Name = "Datum pregleda", Prompt = "Unesite datum pregleda")]
        public DateTime Datum { get; set; }

        [Display(Name = "Anamneza", Prompt = "Unesite anamnezu pacijenta")]
        public string Anamneza { get; set; }

        [Display(Name = "Dijagnoza", Prompt = "Unesite dijagnozu pacijenta")]
        public string Dijagnoza { get; set; }

        public virtual ICollection<PregledSimptom> PregledSimptom { get; set; }
        public virtual ICollection<PregledTerapija> PregledTerapija { get; set; }
    }
}
