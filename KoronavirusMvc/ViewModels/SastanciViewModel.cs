using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class SastanciViewModel
    {
        public IEnumerable<SastanakViewModel> Sastanci { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}