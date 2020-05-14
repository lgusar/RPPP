using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoronavirusMvc.Controllers
{
    public class OrganizacijaController : Controller
    {
        private readonly RPPP09Context ctx;

        public OrganizacijaController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Index()
        {
            var organizacije = ctx.Organizacija
                              .AsNoTracking()
                              .ToList();
            return View(organizacije);
        }
    }
}