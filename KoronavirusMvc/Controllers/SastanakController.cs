using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.Controllers
{
    public class SastanakController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;

        public SastanakController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
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
            var stozeri = ctx.Stozer
                            .OrderBy(d => d.SifraStozera)
                            .Select(d => new { d.Naziv, d.SifraStozera })
                            .ToList();
            ViewBag.Stozeri = new SelectList(stozeri, nameof(Stozer.SifraStozera), nameof(Stozer.Naziv));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sastanak sastanak)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(sastanak);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Sastanak dodan. Šifra sastanka = {sastanak.SifraSastanka}";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    PrepareDropDownLists();
                    return View(sastanak);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(sastanak);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var sastanak = ctx.Sastanak.AsNoTracking().Where(d => d.SifraSastanka == id).SingleOrDefault();
            if (sastanak == null)
            {
                return NotFound("Ne postoji sastanak s oznakom: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                PrepareDropDownLists();
                return View(sastanak);
            }
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Sastanak sastanak = await ctx.Sastanak
                                  .Where(d => d.SifraSastanka == id)
                                  .FirstOrDefaultAsync();
                if (sastanak == null)
                {
                    return NotFound("Neispravna šifra sastanka: " + id);
                }

                if (await TryUpdateModelAsync<Sastanak>(sastanak, "",
                    d => d.SifraStozera, d => d.Datum
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Sastanak ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        PrepareDropDownLists();
                        return View(sastanak);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o sastanku nije moguće povezati s forme");
                    PrepareDropDownLists();
                    return View(sastanak);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int SifraSastanka, int page = 1, int sort = 1, bool ascending = true)
        {
            var sastanak = ctx.Sastanak.Find(SifraSastanka);
            if (sastanak == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    ctx.Remove(sastanak);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Sastanak {sastanak.SifraSastanka} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja sastanka: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Sastanak.Include(z => z.SifraStozeraNavigation).AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Sastanak, object>> orderSelector = null;

            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraSastanka;
                    break;
                case 2:
                    orderSelector = d => d.SifraStozera;
                    break;
                case 3:
                    orderSelector = d => d.Datum;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var sastanci = query
                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();


            var model = new SastanciViewModel
            {
                Sastanci = sastanci,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

    }
}