using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class OpremaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;

        public OpremaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
        }

        [HttpGet]
        public IActionResult Create()
        {
            PrepareDropDownLists();
            return View();
        }

        private void PrepareDropDownLists()
        {
            var institucije = ctx.Institucija.OrderBy(d => d.SifraInstitucije).Select(d => new { d.NazivInstitucije, d.SifraInstitucije }).ToList();
            ViewBag.Institucije = new SelectList(institucije, nameof(Institucija.NazivInstitucije), nameof(Institucija.SifraInstitucije));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Oprema oprema)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(oprema);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Oprema {oprema.SifraOpreme} dodana.";
                    TempData[Constants.ErrorOccured] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    PrepareDropDownLists();
                    return View(oprema);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(oprema);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var oprema = ctx.Oprema
                                .AsNoTracking()
                                .Where(d => d.SifraOpreme == id)
                                .FirstOrDefault();
            if (oprema == null)
            {
                return NotFound($"Ne postoji oprema sa šifrom {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                PrepareDropDownLists();
                return View(oprema);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Oprema oprema = await ctx.Oprema.FindAsync(id);
                if (oprema == null)
                {
                    return NotFound("Ne postoji oprema sa sifrom " + id);
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Oprema>(oprema, "",
                    d => d.SifraInstitucije, d => d.NazivOpreme, d => d.KolicinaOpreme);
                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Oprema{id} ažurirana";
                        TempData[Constants.ErrorOccured] = false;
                        await ctx.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        PrepareDropDownLists();
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(oprema);
                    }
                }
                else
                {
                    PrepareDropDownLists();
                    ModelState.AddModelError(string.Empty, "Podatke o opremi nije moguće povezati s forme");
                    return View(oprema);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccured] = true;
                return RedirectToAction(nameof(Edit), new { id, page, sort, ascending });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int SifraOpreme, int page = 1, int sort = 1, bool ascending = true)
        {
            var oprema = ctx.Oprema.Find(SifraOpreme);
            if (oprema == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    int sifra = oprema.SifraOpreme;
                    ctx.Remove(oprema);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Oprema {sifra} uspješno obrisana.";
                    TempData[Constants.ErrorOccured] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja opreme" + exc.CompleteExceptionMessage();

                    TempData[Constants.ErrorOccured] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Oprema.AsNoTracking();

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
                return RedirectToAction(nameof(Index), new
                {
                    page = pagingInfo.TotalPages,
                    sort,
                    ascending
                });
            }

            System.Linq.Expressions.Expression<Func<Oprema, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraOpreme;
                    break;
                case 2:
                    orderSelector = d => d.SifraInstitucije;
                    break;
                case 3:
                    orderSelector = d => d.NazivOpreme;
                    break;
                case 4:
                    orderSelector = d => d.KolicinaOpreme;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var opremas = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();
            var model = new OpremaViewModel
            {
                Opremas = opremas,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}