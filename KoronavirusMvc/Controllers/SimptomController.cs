using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class SimptomController : Controller
    {
        private readonly RPPP09Context ctx;

        public SimptomController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        } 

        public IActionResult Index()
        {
            var simptomi = ctx.Simptom
                              .AsNoTracking()
                              .ToList();
            return View(simptomi);
        }
    }
}
