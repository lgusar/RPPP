using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class TerapijeViewModel
    {
        public IEnumerable<Terapija> Terapije { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
