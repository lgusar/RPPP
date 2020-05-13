using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class OpremaController : Controller
    {
        private readonly RPPP09Context ctx;

        public OpremaController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Index()
        {
            var oprema = ctx.Oprema
                              .AsNoTracking()
                              .ToList();
            return View(oprema);
        }
    }
}