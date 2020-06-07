using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace KoronavirusMvc.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class InstitucijaOpremaViewModel
    {
        
        public InstitucijaViewModel Institucija { get; set; }

        public OpremeViewModel Oprema { get; set; }

        public PagingInfo PagingInfo { get; set; }

    }
}