using System;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class TerapijaController : Controller
    {
        private readonly RPPP09Context ctx;

        private readonly AppSettings appSettings;

        private readonly ILogger<TerapijaController> logger;

        public TerapijaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<TerapijaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Terapija.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Terapija, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = s => s.SifraTerapije;
                    break;
                case 2:
                    orderSelector = s => s.OpisTerapije;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var terapije = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new TerapijeViewModel
            {
                Terapije = terapije,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Terapija terapija)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    terapija.SifraTerapije = (int)NewId();
                    ctx.Add(terapija);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Terapija {terapija.SifraTerapije} uspješno dodana.";
                    TempData[Constants.ErrorOccurred] = false;

                    logger.LogInformation($"Terapija {terapija.SifraTerapije} uspješno dodana.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    logger.LogError($"Pogreška prilikom dodavanja nove terapije {exc.CompleteExceptionMessage()}");
                    return View(terapija);
                }
            }
            else
            {
                logger.LogError($"Pogreška prilikom dodavanja nove terapije");
                return View(terapija);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int SifraTerapije, int page = 1, int sort = 1, bool ascending = true)
        {
            var terapija = ctx.Terapija.Find(SifraTerapije);
            if (terapija == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    ctx.Remove(terapija);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Terapija {terapija.SifraTerapije} uspješno obrisana.";
                    logger.LogInformation($"Terapija {terapija.SifraTerapije} uspješno obrisana.");
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja terapije." + exc.CompleteExceptionMessage();
                    logger.LogError($"Pogreška prilikom brisanja terapije." + exc.CompleteExceptionMessage());
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var terapija = ctx.Terapija
                             .AsNoTracking()
                             .Where(p => p.SifraTerapije == id)
                             .FirstOrDefault();

            if (terapija == null)
            {
                return NotFound($"Ne postoji terapija s tom šifrom: {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.ascending = ascending;
                return View(terapija);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Terapija terapija = await ctx.Terapija.FindAsync(id);

                if (terapija == null)
                {
                    logger.LogError($"Pogreška prilikom ažuriranja terapije. Ne postoji terapija s tom šifrom: {id}");
                    return NotFound($"Ne postoji terapija s tom šifrom {id}");
                }

                ViewBag.page = page;
                ViewBag.sort = sort;
                ViewBag.ascending = ascending;
                bool ok = await TryUpdateModelAsync<Terapija>(terapija, "", p => p.OpisTerapije);

                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Terapija {terapija.SifraTerapije} uspješno ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;

                        logger.LogInformation($"Terapija {terapija.SifraTerapije} uspješno ažurirana.");

                        await ctx.SaveChangesAsync();

                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        logger.LogError($"Pogreška prilikom ažuriranja terapije. {exc.CompleteExceptionMessage()}");
                        return View(terapija);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o terapiji nije moguće povezati s forme.");
                    logger.LogError($"Pogreška prilikom ažuriranja terapije. Podatke o terapiji nije moguće povezati s forme.");
                    return View(terapija);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                logger.LogError($"Pogreška prilikom ažuriranja terapije. {exc.CompleteExceptionMessage()}");
                TempData[Constants.ErrorOccurred] = true;

                return RedirectToAction(nameof(Edit), new { page, sort, ascending });
            }
        }
        private decimal NewId()
        {
            var maxId = ctx.Terapija
                      .Select(o => o.SifraTerapije)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
    }
}
