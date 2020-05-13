using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class ZarazenaOsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        public ZarazenaOsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;

        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.ZarazenaOsoba.AsNoTracking();

            int count = query.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalItems, sort, ascending });
            }

            System.Linq.Expressions.Expression<Func<ZarazenaOsoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = z => z.IdentifikacijskiBroj;
                    break;
                case 2:
                    orderSelector = z => z.DatZaraze;
                    break;
                case 3:
                    orderSelector = z => z.SifraStanja;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var zarazeneOsobe = query
                            .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new ZarazeneOsobeViewModel
            {
                ZarazeneOsobe = zarazeneOsobe,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}
