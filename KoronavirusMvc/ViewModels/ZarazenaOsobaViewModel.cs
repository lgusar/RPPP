using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class ZarazenaOsobaViewModel
    {
        
        public string IdentifikacijskiBroj { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public DateTime? DatZaraze { get; set; }
        public string NazivStanja { get; set; }
    }
}
