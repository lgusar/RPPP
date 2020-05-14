using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class PutovanjeViewModel
    {
        public IEnumerable<Putovanje> Putovanje { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}