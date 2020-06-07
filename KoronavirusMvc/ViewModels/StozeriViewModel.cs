using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class StozeriViewModel
    {
        public IEnumerable<StozerViewModel> Stozeri { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}