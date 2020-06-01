using KoronavirusMvc.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.ViewModels
{
    public class PregledCreateViewModel
    {
        public Pregled Pregled { get; set; }

        [Required(ErrorMessage = "Identifikacijski broj osobe je obvezno polje")]
        public string idOsoba { get; set; }
    }
}