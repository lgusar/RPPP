using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class OpremaViewModel
    {
        public List<Oprema> Opremas { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
