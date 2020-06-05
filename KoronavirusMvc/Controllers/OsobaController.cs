using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;


namespace KoronavirusMvc.Controllers
{
    public class OsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<OsobaController> logger;
        public OsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<OsobaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Osoba osoba)
        {
            logger.LogTrace(JsonSerializer.Serialize(osoba));
            if (ModelState.IsValid)
            {

                try
                {
                    ctx.Add(osoba);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Osoba {osoba.Ime} {osoba.Prezime} uspješno dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation($"Osoba {osoba.Ime} {osoba.Prezime} dodana");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom dodavanja osobe {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(osoba);
                }
            }
            else
            {
                return View(osoba);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id, int page = 1, int sort = 1, bool ascending = true)
        {
            var osoba = ctx.Osoba.AsNoTracking().Where(o => o.IdentifikacijskiBroj == id).FirstOrDefault();
            if (osoba == null)
            {
                return NotFound($"Ne postoji osoba s identifikacijskim brojem {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(osoba);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Osoba osoba = await ctx.Osoba.FindAsync(id);
                if (osoba == null)
                {
                    return NotFound($"Ne postoji osoba s identifikacijskim brojem {id}");
                }
                logger.LogTrace(JsonSerializer.Serialize(osoba));
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Osoba>(osoba, "", o => o.Ime, o => o.Prezime, o => o.Adresa, o => o.DatRod, o => o.Zanimanje);
                if (ok)
                {
                    try
                    {
                        string punoime = osoba.Ime + " " + osoba.Prezime;
                        TempData[Constants.Message] = $"Podaci osobe {punoime} uspješno ažurirani.";
                        TempData[Constants.ErrorOccurred] = false;
                        await ctx.SaveChangesAsync();
                        logger.LogInformation($"Osoba {osoba.Ime} {osoba.Prezime} ažurirana");
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        logger.LogError($"Pogreška prilikom ažuriranja podataka osobe {exc.CompleteExceptionMessage()}");
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(osoba);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o osobi nije moguće povezati s forme");
                    return View(osoba);
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
        public IActionResult Delete(string id, int page = 1, int sort = 1, bool ascending = true)
        {
            var osoba = ctx.Osoba.AsNoTracking().Where(o => o.IdentifikacijskiBroj == id).SingleOrDefault();
            logger.LogTrace(JsonSerializer.Serialize(osoba));
            if (osoba == null)
            {
                return NotFound($"Osoba s identifikacijski brojem {id} ne postoji.");
            }
            else
            {
                try
                {
                    string punoime = osoba.Ime + " " + osoba.Prezime;
                    ctx.Remove(osoba);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Osoba {punoime} obrisana.",
                        successful = true
                    };
                    logger.LogInformation($"Osoba {osoba.Ime} {osoba.Prezime} obrisana");
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = $"Pogreška prilikom brisanja osobe. {exc.CompleteExceptionMessage()}",
                        successful = false
                    };
                    logger.LogError($"Pogreška prilikom brisanja osobe {exc.CompleteExceptionMessage()}");
                    return Json(result);
                }
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Osoba.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Osoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = o => o.IdentifikacijskiBroj;
                    break;
                case 2:
                    orderSelector = o => o.Ime;
                    break;
                case 3:
                    orderSelector = o => o.Prezime;
                    break;
                case 4:
                    orderSelector = o => o.Adresa;
                    break;
                case 5:
                    orderSelector = o => o.DatRod;
                    break;
                case 6:
                    orderSelector = o => o.Zanimanje;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var osobe = query
                            .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new OsobeViewModel
            {
                Osobe = osobe,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }


            List<KontaktViewModel> kontakti = new List<KontaktViewModel>();
            var kontakt = ctx.Kontakt
                             .Where(k => k.IdOsoba == id)
                             .Select(k => new KontaktViewModel
                             {
                                 IdOsobe = k.IdOsoba,
                                 IdKontakt = k.IdKontakt,
                                 ImeOsoba = k.IdOsobaNavigation.Ime,
                                 PrezimeOsoba = k.IdOsobaNavigation.Prezime,
                                 ImeKontakt = k.IdKontaktNavigation.Ime,
                                 PrezimeKontakt = k.IdKontaktNavigation.Prezime
                             })
                             .ToList();
            if (kontakt.Count != 0)
            {
                foreach (KontaktViewModel k in kontakt)
                {
                    kontakti.Add(k);
                }
            }

            var zarazena = ctx.ZarazenaOsoba
                              .Where(z => z.IdentifikacijskiBroj == id)
                              .FirstOrDefault();
            var osoba = await ctx.Osoba
                            .Where(z => z.IdentifikacijskiBroj == id)
                            .Select(z => new OsobaDetailsViewModel
                            {
                                IdentifikacijskiBroj = z.IdentifikacijskiBroj,
                                Ime = z.Ime,
                                Prezime = z.Prezime,
                                Adresa = z.Adresa,
                                DatRod = z.DatRod,
                                Zanimanje = z.Zanimanje,
                                DatZaraze = z.ZarazenaOsoba.DatZaraze,
                                Zarazena = z.ZarazenaOsoba.IdentifikacijskiBroj.Equals(id) ? true : false,
                                Zarazenastring = z.ZarazenaOsoba.IdentifikacijskiBroj.Equals(id) ? "Da" : "Ne",
                                NazivStanja = z.ZarazenaOsoba.SifraStanjaNavigation.NazivStanja,
                                Kontakti = kontakti,
                                ZarazenaOsoba = z.ZarazenaOsoba
                            })
                            .SingleOrDefaultAsync();

            //var osoba = await ctx.Osoba
            //    .FirstOrDefaultAsync(m => m.IdentifikacijskiBroj == id);
            if (osoba == null)
            {
                return NotFound();
            }


            return View(osoba);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteZarazenaOsoba(string id)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                             .AsNoTracking()
                             .Where(m => m.IdentifikacijskiBroj == id)
                             .SingleOrDefault();
            logger.LogTrace(JsonSerializer.Serialize(zarazenaOsoba));
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
                    logger.LogInformation($"Osoba obrisana");
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja zaražene osobe: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    logger.LogError($"Pogreška prilikom brisanja zaražene osobe {exc.CompleteExceptionMessage()}");
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Zaražena osoba s identifikacijskim brojem {id} ne postoji");
            }
        }


        private void PrepareDropDownLists()
        {
            var stanja = ctx.Stanje.OrderBy(s => s.NazivStanja).Select(s => new { s.NazivStanja, s.SifraStanja }).ToList();
            ViewBag.Stanja = new SelectList(stanja, nameof(Stanje.SifraStanja), nameof(Stanje.NazivStanja));
        }
    }
}