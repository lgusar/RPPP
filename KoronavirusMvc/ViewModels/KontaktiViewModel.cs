using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class KontaktiViewModel
    {
        public IEnumerable<KontaktViewModel> Kontakti { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
