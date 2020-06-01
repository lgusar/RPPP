using KoronavirusMvc.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.ViewModels
{
    public class PregledCreateViewModel
    {
        public Pregled Pregled { get; set; }
        public OsobaPregled OsobaPregled { get; set; }
        public List<string> Simptomi { get; set; }
        public List<string> Terapije { get; set; }
    }
}