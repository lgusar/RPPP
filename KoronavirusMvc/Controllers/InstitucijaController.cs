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
    public class InstitucijaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;

        public InstitucijaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        private async Task PrepareDropDownLists()
        {

            var organizacije = await ctx.Organizacija.OrderBy(d=>d.SifraOrganizacije).Select(d => new { d.SifraOrganizacije, d.Naziv }).ToListAsync();
            ViewBag.Organizacije = new SelectList(organizacije, nameof(Organizacija.Naziv), nameof(Organizacija.SifraOrganizacije));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Institucija institucija)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(institucija);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Institucija {institucija.SifraInstitucije} dodana.";
                    TempData[Constants.ErrorOccured] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(institucija);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(institucija);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var institucija = ctx.Institucija
                                .AsNoTracking()
                                .Where(d => d.SifraInstitucije == id)
                                .FirstOrDefault();
            if (institucija == null)
            {
                return NotFound($"Ne postoji institucija sa šifrom {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                PrepareDropDownLists();
                return View(institucija);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Institucija institucija = await ctx.Institucija.FindAsync(id);
                if (institucija == null)
                {
                    return NotFound("Ne postoji institucija sa sifrom " + id);
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Institucija>(institucija, "",
                    d => d.NazivInstitucije, d => d.RadnoVrijeme, d => d.Kontakt, d => d.SifraOrganizacije);
                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Institucija {id} ažurirana";
                        TempData[Constants.ErrorOccured] = false;
                        await ctx.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(institucija);
                        await PrepareDropDownLists();
                    }
                }
                else
                {
                    await PrepareDropDownLists();
                    ModelState.AddModelError(string.Empty, "Podatke o instituciji nije moguće povezati s forme");
                    return View(institucija);
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
        public IActionResult Delete(int SifraInstitucije, int page = 1, int sort = 1, bool ascending = true)
        {
            var institucija = ctx.Institucija.Find(SifraInstitucije);
            if (institucija == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    int sifra = institucija.SifraInstitucije;
                    ctx.Remove(institucija);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Institucija {sifra} uspješno obrisana.";
                    TempData[Constants.ErrorOccured] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja institucije" + exc.CompleteExceptionMessage();

                    TempData[Constants.ErrorOccured] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Institucija.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Institucija, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraInstitucije;
                    break;
                case 2:
                    orderSelector = d => d.NazivInstitucije;
                    break;
                case 3:
                    orderSelector = d => d.RadnoVrijeme;
                    break;
                case 4:
                    orderSelector = d => d.Kontakt;
                    break;
                case 5:
                    orderSelector = d => d.SifraOrganizacije;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var institucije = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();
            var model = new InstitucijeViewModel
            {
                Institucije = institucije,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}