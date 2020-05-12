using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class PregledController : Controller
    {
        private readonly RPPP09Context ctx;

        public PregledController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        } 

        public IActionResult Index()
        {
            var pregledi = ctx.Pregled
                              .AsNoTracking()
                              .ToList();
            return View(pregledi);
        }
    }
}
