using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public class MasterStatistikaEditViewModel
    {
        public Statistika Statistika { get; set; }
        public SelectList Lokacije { get; set; }
        public SelectList Organizacije { get; set; }
    }

    public class MasterPutovanjeEditViewModel
    {
        public Putovanje Putovanje { get; set; }
        public SelectList Lokacije { get; set; }
        public SelectList Osobe { get; set; }
    }
}
