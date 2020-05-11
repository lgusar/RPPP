using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class PutovanjeLokacija
    {
        public int SifraGrada { get; set; }
        public int SifraPutovanja { get; set; }

        public virtual Lokacija SifraGradaNavigation { get; set; }
        public virtual Putovanje SifraPutovanjaNavigation { get; set; }
    }
}
