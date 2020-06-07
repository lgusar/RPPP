using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KoronavirusMvc.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KoronavirusMvc.Controllers
{
    public class SastanakController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<SastanakController> logger;


        public SastanakController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<SastanakController> logger)
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
            logger.LogTrace(JsonSerializer.Serialize(sastanak), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(sastanak);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(2000), $"Sastanak {sastanak.SifraSastanka} dodan.");
                    TempData[Constants.Message] = $"Sastanak dodan. Šifra sastanka = {sastanak.SifraSastanka}";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog sastanka: {0}", exc.CompleteExceptionMessage());
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

        //[HttpGet]
        //public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    var sastanak = ctx.Sastanak.AsNoTracking().Where(d => d.SifraSastanka == id).SingleOrDefault();
        //    if (sastanak == null)
        //    {
        //        return NotFound("Ne postoji sastanak s oznakom: " + id);
        //    }
        //    else
        //    {
        //        ViewBag.Page = page;
        //        ViewBag.Sort = sort;
        //        ViewBag.Ascending = ascending;
        //        PrepareDropDownLists();
        //        return View(sastanak);
        //    }
        //}

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sastanak = ctx.Sastanak
                             .AsNoTracking()
                             .Where(m => m.SifraSastanka == id)
                             .SingleOrDefault();
            if (sastanak != null)
            {
                PrepareDropDownLists();
                return PartialView(sastanak);
            }
            else
            {
                return NotFound($"Neispravna šifra sastanka: {id}");
            }
        }


        //[HttpPost, ActionName("Edit")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    try
        //    {
        //        Sastanak sastanak = await ctx.Sastanak
        //                          .Where(d => d.SifraSastanka == id)
        //                          .FirstOrDefaultAsync();
        //        if (sastanak == null)
        //        {
        //            return NotFound("Neispravna šifra sastanka: " + id);
        //        }

        //        if (await TryUpdateModelAsync<Sastanak>(sastanak, "",
        //            d => d.SifraStozera, d => d.Datum
        //        ))
        //        {
        //            ViewBag.Page = page;
        //            ViewBag.Sort = sort;
        //            ViewBag.Ascending = ascending;
        //            try
        //            {
        //                await ctx.SaveChangesAsync();
        //                TempData[Constants.Message] = "Sastanak ažuriran.";
        //                TempData[Constants.ErrorOccurred] = false;
        //                return RedirectToAction(nameof(Index), new { page, sort, ascending });
        //            }
        //            catch (Exception exc)
        //            {
        //                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        //                PrepareDropDownLists();
        //                return View(sastanak);
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Podatke o sastanku nije moguće povezati s forme");
        //            PrepareDropDownLists();
        //            return View(sastanak);
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        //        TempData[Constants.ErrorOccurred] = true;
        //        return RedirectToAction(nameof(Edit), id);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sastanak sastanak)
        {
            if (sastanak == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Sastanak.Any(m => m.SifraSastanka == sastanak.SifraSastanka);
            if (!checkId)
            {
                return NotFound($"Neispravna šifra sastanka: {sastanak?.SifraSastanka}");
            }

            PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(sastanak);
                    ctx.SaveChanges();
                    return StatusCode(302, Url.Action(nameof(Row), new { id = sastanak.SifraSastanka }));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(sastanak);
                }
            }
            else
            {
                return PartialView(sastanak);
            }
        }

        public PartialViewResult Row(int id)
        {
            var sastanak = ctx.Sastanak
                             .Where(m => m.SifraSastanka == id)
                             .Select(m => new SastanakViewModel
                             {
                                 NazivStozera = m.SifraStozeraNavigation.Naziv,
                                 Datum = m.Datum,
                                 SifraSastanka = m.SifraSastanka
                             })
                             .SingleOrDefault();
            if (sastanak != null)
            {
                return PartialView(sastanak);
            }
            else
            {
                //vratiti prazan sadržaj?
                return PartialView("ErrorMessageRow", $"Neispravan id sastanka: {id}");
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Delete(int SifraSastanka, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    var sastanak = ctx.Sastanak.Find(SifraSastanka);
        //    if (sastanak == null)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        try
        //        {
        //            ctx.Remove(sastanak);
        //            ctx.SaveChanges();
        //            TempData[Constants.Message] = $"Sastanak {sastanak.SifraSastanka} uspješno obrisan";
        //            TempData[Constants.ErrorOccurred] = false;
        //        }
        //        catch (Exception exc)
        //        {
        //            TempData[Constants.Message] = "Pogreška prilikom brisanja sastanka: " + exc.CompleteExceptionMessage();
        //            TempData[Constants.ErrorOccurred] = true;
        //        }
        //        return RedirectToAction(nameof(Index), new { page, sort, ascending });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var sastanak = ctx.Sastanak
                             .AsNoTracking() //ima utjecaj samo za Update, za brisanje možemo staviti AsNoTracking
                             .Where(m => m.SifraSastanka == id)
                             .SingleOrDefault();
            if (sastanak != null)
            {
                try
                {
                    ctx.Remove(sastanak);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Sastanak sa šifrom {id} obrisan.",
                        successful = true
                    };
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja sastanka: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Sastanak sa šifrom {id} ne postoji");
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
                    orderSelector = d => d.SifraStozeraNavigation.Naziv;
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
                      .Select(m => new SastanakViewModel
                      {
                          NazivStozera = m.SifraStozeraNavigation.Naziv,
                          Datum = m.Datum,
                          SifraSastanka = m.SifraSastanka
                      })
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