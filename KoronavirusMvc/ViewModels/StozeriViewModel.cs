using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class StozeriViewModel
    {
        public IEnumerable<Stozer> Stozeri { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}