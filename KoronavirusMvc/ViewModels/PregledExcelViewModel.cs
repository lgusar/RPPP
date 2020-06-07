using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.ViewModels
{
    public class PregledExcelViewModel
    {
        public int SifraPregleda { get; set; }
        public DateTime Datum { get; set; }
        public string Anamneza { get; set; }
        public string Dijagnoza { get; set; }
    }
}
