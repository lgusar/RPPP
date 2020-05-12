using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class StanjeController : Controller
    {
        private readonly RPPP09Context ctx;
        public StanjeController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }
        public IActionResult Index()
        {
            var stanje = ctx.Stanje
                           .AsNoTracking()
                           .OrderBy(s => s.SifraStanja)
                           .ToList();
            return View(stanje);
        }
    }
}