using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class MjereViewModel
    {
        public IEnumerable<MjeraViewModel> Mjere { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}