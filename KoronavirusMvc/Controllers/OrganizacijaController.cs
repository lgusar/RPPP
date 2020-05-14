using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class OrganizacijaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;

        public OrganizacijaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Organizacija organizacija)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(organizacija);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Organizacija {organizacija.SifraOrganizacije} dodana.";
                    TempData[Constants.ErrorOccured] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(organizacija);
                }
            }
            else
            {
                return View(organizacija);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var organizacija = ctx.Organizacija
                                .AsNoTracking()
                                .Where(d => d.SifraOrganizacije == id)
                                .FirstOrDefault();
            if (organizacija == null)
            {
                return NotFound($"Ne postoji organizacija sa šifrom {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(organizacija);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Organizacija organizacija = await ctx.Organizacija.FindAsync(id);
                if (organizacija == null)
                {
                    return NotFound("Ne postoji organizacija sa sifrom " + id);
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Organizacija>(organizacija, "",
                    d => d.Naziv, d => d.Url);
                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Organizacija {id} ažurirana";
                        TempData[Constants.ErrorOccured] = false;
                        await ctx.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(organizacija);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o organizaciji nije moguće povezati s forme");
                    return View(organizacija);
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
        public IActionResult Delete(int SifraOrganizacije, int page = 1, int sort = 1, bool ascending = true)
        {
            var organizacija = ctx.Organizacija.Find(SifraOrganizacije);
            if (organizacija == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    int sifra = organizacija.SifraOrganizacije;
                    ctx.Remove(organizacija);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Organizacija {sifra} uspješno obrisana.";
                    TempData[Constants.ErrorOccured] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja organizacije" + exc.CompleteExceptionMessage();

                    TempData[Constants.ErrorOccured] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Organizacija.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Organizacija, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraOrganizacije;
                    break;
                case 2:
                    orderSelector = d => d.Naziv;
                    break;
                case 3:
                    orderSelector = d => d.Url;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var organizacije = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();
            var model = new OrganizacijeViewModel
            {
                Organizacije = organizacije,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}