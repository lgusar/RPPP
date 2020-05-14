using KoronavirusMvc.Models;
using System.Collections.Generic;

namespace KoronavirusMvc.ViewModels
{
    public class PreglediViewModel
    {
        public IEnumerable<Pregled> Pregledi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}