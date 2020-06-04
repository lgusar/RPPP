using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class PreporukeViewModel
    {
        public IEnumerable<Preporuka> Preporuke { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
