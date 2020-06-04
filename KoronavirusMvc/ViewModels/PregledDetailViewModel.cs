using KoronavirusMvc.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.ViewModels
{
    public class PregledDetailViewModel
    {
        public Pregled Pregled { get; set; }
        public OsobaPregled OsobaPregled { get; set; }
        public List<Simptom> Simptomi { get; set; }
        public List<Terapija> Terapije { get; set; }
        public Simptom Simptom { get; set; }
        public List<string> TerapijeZaDodavanje { get; set; }
    }
}