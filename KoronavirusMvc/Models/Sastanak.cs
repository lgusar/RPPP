using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Sastanak
    {
        public Sastanak()
        {
            Mjera = new HashSet<Mjera>();
        }

        public int SifraSastanka { get; set; }
        public int SifraStozera { get; set; }
        public DateTime Datum { get; set; }

        public virtual Stozer SifraStozeraNavigation { get; set; }
        public virtual ICollection<Mjera> Mjera { get; set; }
    }
}
