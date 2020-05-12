using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class OsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        public OsobaController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }
        public IActionResult Index()
        {
            var osobe = ctx.Osoba
                           .AsNoTracking()
                           .OrderBy(o => o.IdentifikacijskiBroj)
                           .ToList();
            return View(osobe);
        }
    }
}