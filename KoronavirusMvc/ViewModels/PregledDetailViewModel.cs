using KoronavirusMvc.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.ViewModels
{
    public class PregledDetailViewModel
    {
        public Pregled Pregled { get; set; }

        [Display(Name = "Identifikacijski broj osobe")]
        public string IdOsoba { get; set; }
        public List<Simptom> Simptomi { get; set; }
        public List<Terapija> Terapije { get; set; }
    }
}