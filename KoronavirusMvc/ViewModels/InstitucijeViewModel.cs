using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class InstitucijeViewModel
    {
        public IEnumerable<InstitucijaViewModel> Institucije { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
