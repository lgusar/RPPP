using System;
using System.Collections.Generic;

namespace KoronavirusMvc.Models
{
    public partial class Statistika
    {
        public int SifraObjave { get; set; }
        public int SifraGrada { get; set; }
        public int SifraOrganizacije { get; set; }
        public int BrojSlucajeva { get; set; }
        public int BrojUmrlih { get; set; }
        public int BrojIzlijecenih { get; set; }
        public int BrojAktivnih { get; set; }
        public DateTime Datum { get; set; }

        public virtual Lokacija SifraGradaNavigation { get; set; }
        public virtual Organizacija SifraOrganizacijeNavigation { get; set; }
    }
}
