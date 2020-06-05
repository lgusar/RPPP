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
    public class SimptomController : Controller
    {
        private readonly RPPP09Context ctx;

        private readonly AppSettings appSettings;

        private readonly ILogger<SimptomController> logger;

        public SimptomController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<SimptomController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Simptom.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Simptom, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = s => s.SifraSimptoma;
                    break;
                case 2:
                    orderSelector = s => s.Opis;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var simptomi = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new SimptomiViewModel
            {
                Simptomi = simptomi,
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
        public IActionResult Create(Simptom simptom)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    simptom.SifraSimptoma = (int)NewId();
                    ctx.Add(simptom);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Simptom {simptom.SifraSimptoma} uspješno dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    logger.LogInformation($"Simptom {simptom.SifraSimptoma} uspješno dodan.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    logger.LogError($"Pogreška prilikom dodavanja novog simptoma {exc.CompleteExceptionMessage()}");
                    return View(simptom);
                }
            }
            else
            {
                logger.LogError($"Pogreška prilikom dodavanja novog simptoma");
                return View(simptom);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int SifraSimptoma, int page = 1, int sort = 1, bool ascending = true)
        {
            var simptom = ctx.Simptom.Find(SifraSimptoma);
            if (simptom == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    ctx.Remove(simptom);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Simptom {simptom.SifraSimptoma} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;

                    logger.LogInformation($"Simptom {simptom.SifraSimptoma} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja simptoma." + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;

                    logger.LogError($"Pogreška prilikom brisanja simptoma. {exc.CompleteExceptionMessage()}");
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var simptom = ctx.Simptom
                             .AsNoTracking()
                             .Where(p => p.SifraSimptoma == id)
                             .FirstOrDefault();

            if (simptom == null)
            { 
                return NotFound($"Ne postoji simptom s tom šifrom: {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.ascending = ascending;
                return View(simptom);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Simptom simptom = await ctx.Simptom.FindAsync(id);

                if (simptom == null)
                {
                    logger.LogError($"Pogreška prilikom ažuriranja simptoma. Ne postoji simptom s tom šifrom: {id}");
                    return NotFound($"Ne postoji simptom s tom šifrom {id}");
                }

                ViewBag.page = page;
                ViewBag.sort = sort;
                ViewBag.ascending = ascending;
                bool ok = await TryUpdateModelAsync<Simptom>(simptom, "", p => p.Opis);

                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Simptom {simptom.SifraSimptoma} uspješno ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;

                        await ctx.SaveChangesAsync();

                        logger.LogInformation($"Simptom {simptom.SifraSimptoma} uspješno ažuriran.");

                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        logger.LogError($"Pogreška prilikom ažuriranja simptoma. {exc.CompleteExceptionMessage()}");
                        return View(simptom);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o simptomu nije moguće povezati s forme.");
                    logger.LogError($"Pogreška prilikom ažuriranja simptoma. Podatke o simptomu nije moguće povezati s forme.");
                    return View(simptom);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;

                logger.LogError($"Pogreška prilikom ažuriranja simptoma. {exc.CompleteExceptionMessage()}");

                return RedirectToAction(nameof(Edit), new { page, sort, ascending });
            }
        }

        private decimal NewId()
        {
            var maxId = ctx.Simptom
                      .Select(o => o.SifraSimptoma)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
    }
}
