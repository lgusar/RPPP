using KoronavirusMvc.Models;
using System;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class SastanakViewModel
    {
        public int SifraSastanka { get; set; }

        public string NazivStozera { get; set; }

        public DateTime Datum { get; set; }

        public virtual Stozer SifraStozeraNavigation { get; set; }

        public virtual ICollection<Mjera> Mjera { get; set; }
    }
}