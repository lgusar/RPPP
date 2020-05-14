using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class PregledController : Controller
    {
        private readonly RPPP09Context ctx;

        private readonly AppSettings appSettings;

        public PregledController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string SifraPregleda, int page = 1, int sort = 1, bool ascending = true)
        {
            var pregled = ctx.Pregled.Find(Int32.Parse(SifraPregleda));
            if (pregled == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    ctx.Remove(pregled);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Pregled {pregled.SifraPregleda} uspješno obrisan.";
                    TempData[Constants.ErrorOccured] = false;
                }
                catch(Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja pregleda." + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccured] = true;
                }
                return RedirectToAction(nameof(Index), new {page, sort, ascending});
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pregled pregled)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    pregled.SifraPregleda = (int)NewId();
                    ctx.Add(pregled);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Pregled {pregled.SifraPregleda} uspješno dodan.";
                    TempData[Constants.ErrorOccured] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(pregled);
                }
            }
            else
            {
                return View(pregled);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var pregled = ctx.Pregled
                             .AsNoTracking()
                             .Where(p => p.SifraPregleda == id)
                             .FirstOrDefault();

            if (pregled == null)
            {
                return NotFound($"Ne postoji pregled s tom šifrom: {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.ascending = ascending;
                return View(pregled);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Pregled pregled = await ctx.Pregled.FindAsync(id);

                if (pregled == null)
                {
                    return NotFound($"Ne postoji pregled s tom šifrom {id}");
                }

                ViewBag.page = page;
                ViewBag.sort = sort;
                ViewBag.ascending = ascending;
                bool ok = await TryUpdateModelAsync<Pregled>(pregled, "", p => p.Datum, p => p.Anamneza, p => p.Dijagnoza);

                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Pregled {pregled.SifraPregleda} uspješno ažuriran.";
                        TempData[Constants.ErrorOccured] = false;

                        await ctx.SaveChangesAsync();

                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch(Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(pregled);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o pregledu nije moguće povezati s forme.");
                    return View(pregled);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccured] = true;

                return RedirectToAction(nameof(Edit), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Pregled.AsNoTracking();

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
                return RedirectToAction(nameof(Index), new { 
                    page = pagingInfo.TotalPages,
                    sort,
                    ascending
                });
            }

            System.Linq.Expressions.Expression<Func<Pregled, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.SifraPregleda;
                    break;
                case 2:
                    orderSelector = p => p.Datum;
                    break;
                case 3:
                    orderSelector = p => p.Anamneza;
                    break;
                case 4:
                    orderSelector = p => p.Dijagnoza;
                    break;

            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var pregledi = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new PreglediViewModel
            {
                Pregledi = pregledi,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        private decimal NewId()
        {
            var maxId = ctx.Pregled
                      .Select(o => o.SifraPregleda)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
    }
}
