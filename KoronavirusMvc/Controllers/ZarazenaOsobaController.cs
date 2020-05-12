using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class ZarazenaOsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        public ZarazenaOsobaController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }
        public IActionResult Index()
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                           .AsNoTracking()
                           .OrderBy(z => z.SifraStanja)
                           .ToList();
            return View(zarazenaOsoba);
        }
    }
}
