using KoronavirusMvc.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoronavirusMvc.ViewModels
{
    public class PregledCreateViewModel
    {
        public Pregled Pregled { get; set; }
        public OsobaPregled OsobaPregled { get; set; }
    }
}