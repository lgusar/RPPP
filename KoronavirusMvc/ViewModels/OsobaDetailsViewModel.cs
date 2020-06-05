using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class OsobaDetailsViewModel
    {
        public string IdentifikacijskiBroj { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Adresa { get; set; }
        public DateTime DatRod { get; set; }
        public string Zanimanje { get; set; }
        public DateTime? DatZaraze { get; set; }
        public bool Zarazena { get; set; }
        public string Zarazenastring { get; set; }
        public string NazivStanja { get; set; }

        public List<KontaktViewModel> Kontakti;
        public PagingInfo PagingInfo;
        public ZarazenaOsoba ZarazenaOsoba;

    }
}
