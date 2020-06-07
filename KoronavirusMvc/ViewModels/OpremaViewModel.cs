using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class OpremaViewModel
    {

        public int SifraOpreme { get; set; }


        public string NazivInstitucije { get; set; }

        public string NazivOpreme { get; set; }

        public int KolicinaOpreme { get; set; }

    }
}
