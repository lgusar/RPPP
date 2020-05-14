using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class OrganizacijeViewModel
    {
        public IEnumerable<Organizacija> Organizacije { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
