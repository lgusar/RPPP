using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class PreporukaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;

        public PreporukaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
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
            var organizacije = ctx.Organizacija.OrderBy(d => d.SifraOrganizacije).Select(d => new { d.SifraOrganizacije, d.Naziv }).ToList();
            ViewBag.Organizacije = new SelectList(organizacije, nameof(Organizacija.Naziv), nameof(Organizacija.SifraOrganizacije));

            var preporuke = ctx.Preporuka.OrderBy(d => d.SifraPreporuke).Select(d => new { d.SifraPreporuke, d.Opis }).ToList();
            ViewBag.Preporuke = new SelectList(preporuke, nameof(Preporuka.Opis), nameof(Preporuka.SifraPreporuke));


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create (Preporuka preporuka)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(preporuka);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Preporuka {preporuka.SifraPreporuke} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    PrepareDropDownLists();
                    return View(preporuka);
                }
            }
            else {
                PrepareDropDownLists();
                return View(preporuka);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var preporuka = ctx.Preporuka
                                .AsNoTracking()
                                .Where(d => d.SifraPreporuke == id)
                                .FirstOrDefault();
            if (preporuka == null)
            {
                return NotFound($"Ne postoji preporuka sa šifrom {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                PrepareDropDownLists();
                return View(preporuka);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Preporuka preporuka = await ctx.Preporuka.FindAsync(id);
                if (preporuka == null)
                {
                    return NotFound("Ne postoji preporuka sa sifrom " + id);
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Preporuka>(preporuka, "",
                    d => d.SifraOrganizacije, d => d.SifraStozera, d => d.SifraPrethodnePreporuke, d => d.Opis, d => d.VrijemeObjave);
                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Preporuka {id} ažurirana";
                        TempData[Constants.ErrorOccurred] = false;
                        await ctx.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        PrepareDropDownLists();
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(preporuka);
                    }
                }
                else
                {
                    PrepareDropDownLists();
                    ModelState.AddModelError(string.Empty, "Podatke o preporuci nije moguće povezati s forme");
                    return View(preporuka);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), new { id, page, sort, ascending });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int SifraPreporuke, int page = 1, int sort=1, bool ascending = true) {
            var preporuka = ctx.Preporuka.Find(SifraPreporuke);
            if (preporuka == null)
            {
                return NotFound();
            }
            else {
                try {
                    int sifra = preporuka.SifraPreporuke;
                    ctx.Remove(preporuka);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Preporuka {sifra} uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch(Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja preporuke" + exc.CompleteExceptionMessage();

                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Preporuka.AsNoTracking();

            int count = query.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (page > pagingInfo.TotalPages) {
                return RedirectToAction(nameof(Index), new
                {
                    page = pagingInfo.TotalPages,
                    sort,
                    ascending
                });
            }

            System.Linq.Expressions.Expression<Func<Preporuka, object>> orderSelector = null;
            switch (sort) {
                case 1:
                    orderSelector = d => d.SifraPreporuke;
                    break;
                case 2:
                    orderSelector = d => d.SifraOrganizacije;
                    break;
                case 3:
                    orderSelector = d => d.SifraStozera;
                    break;
                case 4:
                    orderSelector = d => d.SifraPrethodnePreporuke;
                    break;
                case 5:
                    orderSelector = d => d.Opis;
                    break;
                case 6:
                    orderSelector = d => d.VrijemeObjave;
                    break;
            }

            if (orderSelector != null) {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }
            
            var preporuke = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();
            var model = new PreporukeViewModel
            {
                Preporuke = preporuke,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}