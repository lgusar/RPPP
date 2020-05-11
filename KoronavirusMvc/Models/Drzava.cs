using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Drzava
    {
        public Drzava()
        {
            Lokacija = new HashSet<Lokacija>();
        }

        public string SifraDrzave { get; set; }
        public string ImeDrzave { get; set; }

        public virtual ICollection<Lokacija> Lokacija { get; set; }
    }
}
