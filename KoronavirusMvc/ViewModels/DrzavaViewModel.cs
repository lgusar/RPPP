using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class DrzavaViewModel
    {
        public IEnumerable<Drzava> Drzava { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
