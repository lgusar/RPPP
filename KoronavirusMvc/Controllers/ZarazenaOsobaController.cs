using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class ZarazenaOsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        public ZarazenaOsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;

        }

        //[HttpGet]
        //public async Task<IActionResult> Create()
        //{
        //    await PrepareDropdownLists();
        //    return View();
        //}

        private void PrepareDropDownLists()
        {
            var stanja = ctx.Stanje.OrderBy(s => s.NazivStanja).Select(s => new { s.NazivStanja, s.SifraStanja }).ToList();
            ViewBag.Stanja = new SelectList(stanja, nameof(Stanje.SifraStanja), nameof(Stanje.NazivStanja));
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(ZarazenaOsoba zarazenaOsoba)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        try
        //        {
        //            ctx.Add(zarazenaOsoba);
        //            ctx.SaveChanges();
        //            TempData[Constants.Message] = $"Osoba {zarazenaOsoba.IdentifikacijskiBroj} uspješno dodana u listu zaraženih osoba. ";
        //            TempData[Constants.ErrorOccurred] = false;

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception exc)
        //        {
        //            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        //            await PrepareDropdownLists();
        //            return View(zarazenaOsoba);
        //        }
        //    }
        //    else
        //    {
        //        await PrepareDropdownLists();
        //        return View(zarazenaOsoba);
        //    }
        //}

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                             .Include(o => o.IdentifikacijskiBrojNavigation)
                             .AsNoTracking()
                             .Where(m => m.IdentifikacijskiBroj == id)
                             .SingleOrDefault();
            if (zarazenaOsoba != null)
            {
                PrepareDropDownLists();
                return PartialView(zarazenaOsoba);
            }
            else
            {
                return NotFound($"Neispravan id mjesta: {id}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ZarazenaOsoba zarazenaOsoba)
        {
            if (zarazenaOsoba == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.ZarazenaOsoba.Any(m => m.IdentifikacijskiBroj == zarazenaOsoba.IdentifikacijskiBroj);
            if (!checkId)
            {
                return NotFound($"Neispravan identifikacijski broj zarazene osobe: {zarazenaOsoba?.IdentifikacijskiBroj}");
            }

            PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(zarazenaOsoba);
                    ctx.SaveChanges();
                    return StatusCode(302, Url.Action(nameof(Row), new { id = zarazenaOsoba.IdentifikacijskiBroj }));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(zarazenaOsoba);
                }
            }
            else
            {
                return PartialView(zarazenaOsoba);
            }
        }

        public PartialViewResult Row(string id)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                                    .Where(z => z.IdentifikacijskiBroj == id)
                                    .Select(z => new ZarazenaOsobaViewModel
                                    {
                                        IdentifikacijskiBroj = z.IdentifikacijskiBroj,
                                        Ime = z.IdentifikacijskiBrojNavigation.Ime,
                                        Prezime = z.IdentifikacijskiBrojNavigation.Prezime,
                                        DatZaraze = z.DatZaraze,
                                        NazivStanja = z.SifraStanjaNavigation.NazivStanja
                                    })
                                    .SingleOrDefault();
            if(zarazenaOsoba != null)
            {
                return PartialView(zarazenaOsoba);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan identifikacijski broj osobe.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                             .AsNoTracking() 
                             .Where(m => m.IdentifikacijskiBroj == id)
                             .SingleOrDefault();
            if (zarazenaOsoba != null)
            {
                try
                {
                    
                    ctx.Remove(zarazenaOsoba);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Zaražena osoba obrisana.",
                        successful = true
                    };
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja zaražene osobe: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Zaražena osoba s identifikacijskim brojem {id} ne postoji");
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.ZarazenaOsoba.AsNoTracking();

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
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalItems, sort, ascending });
            }

            System.Linq.Expressions.Expression<Func<ZarazenaOsoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = z => z.IdentifikacijskiBroj;
                    break;
                case 2:
                    orderSelector = z => z.IdentifikacijskiBrojNavigation.Ime;
                    break;
                case 3:
                    orderSelector = z => z.IdentifikacijskiBrojNavigation.Prezime;
                    break;
                case 4:
                    orderSelector = z => z.DatZaraze;
                    break;
                case 5:
                    orderSelector = z => z.SifraStanjaNavigation.NazivStanja;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var zarazeneOsobe = query
                            .Select(z => new ZarazenaOsobaViewModel
                            {
                                IdentifikacijskiBroj = z.IdentifikacijskiBroj,
                                Ime = z.IdentifikacijskiBrojNavigation.Ime,
                                Prezime = z.IdentifikacijskiBrojNavigation.Prezime,
                                DatZaraze = z.DatZaraze,
                                NazivStanja = z.SifraStanjaNavigation.NazivStanja
                            })
                            .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new ZarazeneOsobeViewModel
            {
                ZarazeneOsobe = zarazeneOsobe,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}
