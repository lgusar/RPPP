using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace KoronavirusMvc.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class InstitucijaViewModel
    {
        
        public int SifraInstitucije { get; set; }

        [Display(Name = "Naziv institucije")]
        public string NazivInstitucije { get; set; }

        [Display(Name = "Radno Vrijeme")]
        public string RadnoVrijeme { get; set; }
        
        [Display(Name = "Kontakt")]
        public string Kontakt { get; set; }

        [Display(Name = "Organizacija")]
        public string NazivOrganizacije { get; set; }

    }
}