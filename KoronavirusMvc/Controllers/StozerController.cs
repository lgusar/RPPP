using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoronavirusMvc.Controllers
{
    public class StozerController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<DrzavaController> logger;

        public StozerController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<DrzavaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
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
            var osobe = ctx.Osoba
                            .OrderBy(d => d.IdentifikacijskiBroj)
                            .Select(d => new
                            {
                                IdentifikacijskiBroj = d.IdentifikacijskiBroj,
                                imePrezime = string.Format("{0} {1}", d.Ime, d.Prezime)
                            })
                            .ToList();
            ViewBag.Osobe = new SelectList(osobe, nameof(Osoba.IdentifikacijskiBroj), nameof(Osoba.imePrezime));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Stozer stozer)
        {
            logger.LogTrace(JsonSerializer.Serialize(stozer), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(stozer);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Stožer {stozer.Naziv} dodan.");
                    TempData[Constants.Message] = $"Stožer {stozer.Naziv} dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog stožera: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    PrepareDropDownLists();
                    return View(stozer);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(stozer);
            }
        }


        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var stozer = ctx.Stozer.AsNoTracking().Where(d => d.SifraStozera == id).SingleOrDefault();
            if (stozer == null)
            {
                return NotFound("Ne postoji stožer s oznakom: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                PrepareDropDownLists();
                return View(stozer);
            }
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Stozer stozer = await ctx.Stozer
                                  .Where(d => d.SifraStozera == id)
                                  .FirstOrDefaultAsync();
                if (stozer == null)
                {
                    return NotFound("Neispravna šifra stožera: " + id);
                }

                if (await TryUpdateModelAsync<Stozer>(stozer, "",
                    d => d.Naziv, d => d.IdPredsjednika
                ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Stožer ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        PrepareDropDownLists();
                        return View(stozer);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o stožeru nije moguće povezati s forme");
                    PrepareDropDownLists();
                    return View(stozer);
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
        public IActionResult Delete(int SifraStozera, int page = 1, int sort = 1, bool ascending = true)
        {
            var stozer = ctx.Stozer.Find(SifraStozera);
            if (stozer == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    string naziv = stozer.Naziv;
                    ctx.Remove(stozer);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Stožer {naziv} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja stožera: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Stozer.Include(z => z.IdPredsjednikaNavigation).AsNoTracking();

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