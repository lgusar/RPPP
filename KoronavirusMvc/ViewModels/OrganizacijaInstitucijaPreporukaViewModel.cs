using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace KoronavirusMvc.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class OrganizacijaInstitucijaPreporukaViewModel
    {
        
        public Organizacija Organizacija { get; set; }
        public InstitucijeViewModel Institucije { get; set; }

        public PreporukeViewModel Preporuke { get; set; }

        public PagingInfo PagingInfo { get; set; }

    }
}