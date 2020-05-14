﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.Models
{
    public partial class Oprema
    {

        [Display(Name = "Šifra opreme", Prompt = "Unesite šifru opreme")]
        [Required(ErrorMessage = "Šifra opreme je obavezno polje")]
        public int SifraOpreme { get; set; }

        [Display(Name = "Šifra institucije", Prompt = "Unesite šifru institucije")]
        [Required(ErrorMessage = "Šifra Institucije je obavezno polje")]
        public int SifraInstitucije { get; set; }

        [Display(Name = "Naziv opreme", Prompt = "Unesite naziv opreme")]
        [Required(ErrorMessage = "Naziv opreme je obavezno polje")]
        public string NazivOpreme { get; set; }

        [Display(Name = "Količina opreme", Prompt = "Unesite količinu opreme")]
        [Required(ErrorMessage = "Količina opreme je obavezno polje")]
        public int KolicinaOpreme { get; set; }

        public virtual Institucija SifraInstitucijeNavigation { get; set; }
    }
}
