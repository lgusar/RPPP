using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class KontaktController : Controller
    {
        private readonly RPPP09Context ctx;
        public KontaktController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }
        public IActionResult Index()
        {
            var kontakt = ctx.Kontakt
                           .AsNoTracking()
                           .OrderBy(k => k.IdOsoba)
                           .ToList();
            return View(kontakt);
        }
    }
}