using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class OsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        public OsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
            
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Osoba.AsNoTracking();

            int count = query.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if(page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalItems, sort, ascending });
            }

            System.Linq.Expressions.Expression<Func<Osoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = o => o.IdentifikacijskiBroj;
                    break;
                case 2:
                    orderSelector = o => o.Ime;
                    break;
                case 3:
                    orderSelector = o => o.Prezime;
                    break;
                case 4:
                    orderSelector = o => o.Adresa;
                    break;
                case 5:
                    orderSelector = o => o.DatRod;
                    break;
                case 6:
                    orderSelector = o => o.Zanimanje;
                    break;
            }

            if(orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var osobe = query
                            .Skip((page -1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new OsobeViewModel
            {
                Osobe = osobe,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}