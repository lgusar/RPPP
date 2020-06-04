using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace KoronavirusMvc.ViewModels
{
    public class KontaktViewModel
    {
        public string IdOsobe { get; set; }
        public string IdKontakt { get; set; }
        public string ImeOsoba { get; set; }
        public string PrezimeOsoba { get; set; }
        public string ImeKontakt { get; set; }
        public string PrezimeKontakt { get; set; }
    }
}
