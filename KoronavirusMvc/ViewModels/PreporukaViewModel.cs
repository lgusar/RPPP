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
    public class PreporukaViewModel
    {
        public string Opis { get; set; }

        public int SifraPreporuke { get; set; }

        public string NazivOrganizacije { get; set; }

        public string NazivStozera { get; set; }

        public string OpisPrethodnePreporuke { get; set; }


        public DateTime VrijemeObjave { get; set; }
    }
}
