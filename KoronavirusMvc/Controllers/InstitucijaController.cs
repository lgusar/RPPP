using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class InstitucijaController : Controller
    {
        private readonly RPPP09Context ctx;

        public InstitucijaController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Index()
        {
            var institucije = ctx.Institucija
                              .AsNoTracking()
                              .ToList();
            return View(institucije);
        }
    }
}