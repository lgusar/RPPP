using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace KoronavirusMvc.Controllers
{
    public class StozerController : Controller
    {

        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;

        public StozerController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Stozer.AsNoTracking();

            int count = query.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (page > pagingInfo.TotalItems)
            {
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages, sort, ascending });
            }

            System.Linq.Expressions.Expression<Func<Stozer, object>> orderSelector = null;

            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraStozera;
                    break;
                case 2:
                    orderSelector = d => d.Naziv;
                    break;
                case 3:
                    orderSelector = d => d.IdPredsjednika;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var stozeri = query
                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();


            var model = new StozeriViewModel
            {
                Stozeri = stozeri,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
    }
}