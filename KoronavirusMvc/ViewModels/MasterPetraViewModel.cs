using KoronavirusMvc.Models;
using System.Collections;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class MasterPetraViewModel
    {

        public IEnumerable<Statistika> Statistike { get; set; }
        public IEnumerable<Lokacija> Lokacije { get; set; }
        public IEnumerable<Drzava> Drzave { get; set; }
        public IEnumerable<Putovanje> Putovanja { get; set; }

        public Drzava OdabranaDrzava { get; set; }
    }
}
