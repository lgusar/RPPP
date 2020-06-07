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
        private readonly ILogger<StozerController> logger;

        public StozerController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<StozerController> logger)
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

        /// <summary>
        /// 
        /// </summary>
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


        //[HttpGet]
        //public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    var stozer = ctx.Stozer.AsNoTracking().Where(d => d.SifraStozera == id).SingleOrDefault();
        //    if (stozer == null)
        //    {
        //        return NotFound("Ne postoji stožer s oznakom: " + id);
        //    }
        //    else
        //    {
        //        ViewBag.Page = page;
        //        ViewBag.Sort = sort;
        //        ViewBag.Ascending = ascending;
        //        PrepareDropDownLists();
        //        return View(stozer);
        //    }
        //}


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var stozer = ctx.Stozer
                             .AsNoTracking()
                             .Where(m => m.SifraStozera == id)
                             .SingleOrDefault();
            if (stozer != null)
            {
                PrepareDropDownLists();
                return PartialView(stozer);
            }
            else
            {
                return NotFound($"Neispravna šifra stožera: {id}");
            }
        }


        //[HttpPost, ActionName("Edit")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    try
        //    {
        //        Stozer stozer = await ctx.Stozer
        //                          .Where(d => d.SifraStozera == id)
        //                          .FirstOrDefaultAsync();
        //        if (stozer == null)
        //        {
        //            return NotFound("Neispravna šifra stožera: " + id);
        //        }

        //        if (await TryUpdateModelAsync<Stozer>(stozer, "",
        //            d => d.Naziv, d => d.IdPredsjednika
        //        ))
        //        {
        //            ViewBag.Page = page;
        //            ViewBag.Sort = sort;
        //            ViewBag.Ascending = ascending;
        //            try
        //            {
        //                await ctx.SaveChangesAsync();
        //                TempData[Constants.Message] = "Stožer ažuriran.";
        //                TempData[Constants.ErrorOccurred] = false;
        //                return RedirectToAction(nameof(Index), new { page, sort, ascending });
        //            }
        //            catch (Exception exc)
        //            {
        //                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        //                PrepareDropDownLists();
        //                return View(stozer);
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Podatke o stožeru nije moguće povezati s forme");
        //            PrepareDropDownLists();
        //            return View(stozer);
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
        public IActionResult Edit(Stozer stozer)
        {
            if (stozer == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Stozer.Any(m => m.SifraStozera == stozer.SifraStozera);
            if (!checkId)
            {
                return NotFound($"Neispravna šifra stožera: {stozer?.SifraStozera}");
            }

            PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(stozer);
                    ctx.SaveChanges();
                    return StatusCode(302, Url.Action(nameof(Row), new { id = stozer.SifraStozera }));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(stozer);
                }
            }
            else
            {
                return PartialView(stozer);
            }
        }

        public PartialViewResult Row(int id)
        {
            var stozer = ctx.Stozer
                             .Where(m => m.SifraStozera == id)
                             .Select(m => new StozerViewModel
                             {
                                 ImePredsjednika = m.IdPredsjednikaNavigation.Ime + m.IdPredsjednikaNavigation.Prezime,
                                 SifraStozera = m.SifraStozera,
                                 Naziv = m.Naziv
                             })
                             .SingleOrDefault();
            if (stozer != null)
            {
                return PartialView(stozer);
            }
            else
            {
                //vratiti prazan sadržaj?
                return PartialView("ErrorMessageRow", $"Neispravan id stozera: {id}");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var stozer = ctx.Stozer
                             .AsNoTracking() //ima utjecaj samo za Update, za brisanje možemo staviti AsNoTracking
                             .Where(m => m.SifraStozera == id)
                             .SingleOrDefault();
            if (stozer != null)
            {
                try
                {
                    string naziv = stozer.Naziv;
                    ctx.Remove(stozer);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Stožer {naziv} sa šifrom {id} obrisan.",
                        successful = true
                    };
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja stožera: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Stožer sa šifrom {id} ne postoji");
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Delete(int SifraStozera, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    var stozer = ctx.Stozer.Find(SifraStozera);
        //    if (stozer == null)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        try
        //        {
        //            string naziv = stozer.Naziv;
        //            ctx.Remove(stozer);
        //            ctx.SaveChanges();
        //            TempData[Constants.Message] = $"Stožer {naziv} uspješno obrisan";
        //            TempData[Constants.ErrorOccurred] = false;
        //        }
        //        catch (Exception exc)
        //        {
        //            TempData[Constants.Message] = "Pogreška prilikom brisanja stožera: " + exc.CompleteExceptionMessage();
        //            TempData[Constants.ErrorOccurred] = true;
        //        }
        //        return RedirectToAction(nameof(Index), new { page, sort, ascending });
        //    }
        //}

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
                    orderSelector = d => d.Naziv;
                    break;
                case 2:
                    orderSelector = d => d.IdPredsjednikaNavigation.Ime + d.IdPredsjednikaNavigation.Prezime;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var stozeri = query
                      .Select(m => new StozerViewModel
                      {
                          ImePredsjednika = m.IdPredsjednikaNavigation.Ime + m.IdPredsjednikaNavigation.Prezime,
                          SifraStozera = m.SifraStozera,
                          Naziv = m.Naziv
                      })
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