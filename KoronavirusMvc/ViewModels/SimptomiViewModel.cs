using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class SimptomiViewModel
    {
        public IEnumerable<Simptom> Simptomi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
