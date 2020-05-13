using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KoronavirusMvc.Models;

namespace KoronavirusMvc.Controllers
{
    public class PutovanjeController : Controller
    {
        private readonly RPPP09Context _context;

        public PutovanjeController(RPPP09Context context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            var putovanja = _context.Putovanje
                                    .AsNoTracking()
                                    .ToList();
            return View(putovanja);
        }
    }
}
