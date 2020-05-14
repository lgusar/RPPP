using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdownLists();
            return View();
        }

        private async Task PrepareDropdownLists()
        {
            var stanja = await ctx.Stanje.OrderBy(s => s.NazivStanja).Select(s => new { s.NazivStanja, s.SifraStanja }).ToListAsync();
            ViewBag.Stanja = new SelectList(stanja, nameof(Stanje.SifraStanja), nameof(Stanje.NazivStanja));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ZarazenaOsoba zarazenaOsoba)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    ctx.Add(zarazenaOsoba);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Osoba {zarazenaOsoba.IdentifikacijskiBroj} uspješno dodana u listu zaraženih osoba. ";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropdownLists();
                    return View(zarazenaOsoba);
                }
            }
            else
            {
                await PrepareDropdownLists();
                return View(zarazenaOsoba);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string IdentifikacijskiBroj, int page = 1, int sort = 1, bool ascending = true)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba.Find(IdentifikacijskiBroj);
            if (zarazenaOsoba == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    
                    ctx.Remove(zarazenaOsoba);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Osoba uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja osobe: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.ZarazenaOsoba.Include(z => z.SifraStanjaNavigation).Include(z => z.IdentifikacijskiBrojNavigation).AsNoTracking();

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
                    orderSelector = z => z.IdentifikacijskiBrojNavigation.Ime;
                    break;
                case 3:
                    orderSelector = z => z.IdentifikacijskiBrojNavigation.Prezime;
                    break;
                case 4:
                    orderSelector = z => z.DatZaraze;
                    break;
                case 5:
                    orderSelector = z => z.SifraStanjaNavigation.NazivStanja;
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
