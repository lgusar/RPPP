using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class PreporukaController : Controller
    {
        private readonly RPPP09Context ctx;

        public PreporukaController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Index()
        {
            var preporuke = ctx.Preporuka
                              .AsNoTracking()
                              .ToList();
            return View(preporuke);
        }
    }
}