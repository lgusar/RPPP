using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KoronavirusMvc.Controllers
{
    public class StozerController : Controller
    {

        private readonly RPPP09Context ctx;

        public StozerController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Index()
        {
            var stozeri = ctx.Stozer
                .AsNoTracking()
                .OrderBy(d => d.SifraStozera).
                ToList();

            return View(stozeri);
        }
    }
}