﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class PregledController : Controller
    {
        private readonly RPPP09Context ctx;

        private readonly AppSettings appSettings;

        public PregledController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string SifraPregleda, int page = 1, int sort = 1, bool ascending = true)
        {
            var pregled = ctx.Pregled.Find(Int32.Parse(SifraPregleda));
            if (pregled == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    ctx.Remove(pregled);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Pregled {pregled.SifraPregleda} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja pregleda." + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PregledCreateViewModel pregledCreate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(ctx.Osoba.Find(pregledCreate.idOsoba) != null)
                    {
                        pregledCreate.Pregled.SifraPregleda = (int)NewId();
                        ctx.Add(pregledCreate.Pregled);
                        ctx.OsobaPregled.Add(new OsobaPregled
                        {
                            IdentifikacijskiBroj = pregledCreate.idOsoba,
                            SifraPregleda = pregledCreate.Pregled.SifraPregleda
                        });
                        ctx.SaveChanges();

                        TempData[Constants.Message] = $"Pregled {pregledCreate.Pregled.SifraPregleda} uspješno dodan.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index));
                    }

                    else
                    {
                        TempData[Constants.Message] = $"Ne postoji osoba s tim identifikacijskim brojem.";
                        TempData[Constants.ErrorOccurred] = true;
                        return View(pregledCreate);
                    }
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(pregledCreate);
                }
            }
            else
            {
                return View(pregledCreate);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var pregled = ctx.Pregled
                             .AsNoTracking()
                             .Where(p => p.SifraPregleda == id)
                             .FirstOrDefault();

            var idOsoba = ctx.OsobaPregled.AsNoTracking()
                             .Where(p => p.SifraPregleda == id)
                             .FirstOrDefault();

            if (pregled == null)
            {
                return NotFound($"Ne postoji pregled s tom šifrom: {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.ascending = ascending;

                string ident = "";

                if (idOsoba == null)
                {
                    ident = "Nema ident. broja osobe";
                }
                else
                {
                    ident = idOsoba.IdentifikacijskiBroj;
                }

                return View(new PregledCreateViewModel { 
                    Pregled = pregled,
                    idOsoba = ident
                });
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(PregledCreateViewModel model, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Pregled pregled = await ctx.Pregled.FindAsync(model.Pregled.SifraPregleda);
                OsobaPregled op = await ctx.OsobaPregled.FindAsync(model.Pregled.SifraPregleda);

                if (pregled == null)
                {
                    return NotFound($"Ne postoji pregled s tom šifrom {model.Pregled.SifraPregleda}");
                }

                ViewBag.page = page;
                ViewBag.sort = sort;
                ViewBag.ascending = ascending;

                if (ctx.Osoba.Find(model.idOsoba) != null) {
                    ctx.Pregled.Update(model.Pregled);
                    op.IdentifikacijskiBroj = model.idOsoba;
                    ctx.OsobaPregled.Update(op);

                    try
                    {
                        TempData[Constants.Message] = $"Pregled {pregled.SifraPregleda} uspješno ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;

                        await ctx.SaveChangesAsync();

                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ne postoji osoba s tim identifikacijskim brojem.");
                    return View(model);
                }

            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;

                return RedirectToAction(nameof(Edit), new { page, sort, ascending });
            }
        }

        /*[HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Pregled pregled = await ctx.Pregled.FindAsync(id);
                OsobaPregled op = await ctx.OsobaPregled.FindAsync(id);

                if (pregled == null)
                {
                    return NotFound($"Ne postoji pregled s tom šifrom {id}");
                }

                ViewBag.page = page;
                ViewBag.sort = sort;
                ViewBag.ascending = ascending;
                bool ok = await TryUpdateModelAsync<Pregled>(pregled, "", p => p.Datum, p => p.Anamneza, p => p.Dijagnoza);

                ok = await TryUpdateModelAsync<OsobaPregled>(op, "", p => p.IdentifikacijskiBroj);

                if (ok)
                {
                    if (ctx.Osoba.Find(op.IdentifikacijskiBroj) != null)
                    {
                        try
                        {
                            TempData[Constants.Message] = $"Pregled {pregled.SifraPregleda} uspješno ažuriran.";
                            TempData[Constants.ErrorOccurred] = false;

                            await ctx.SaveChangesAsync();

                            return RedirectToAction(nameof(Index), new { page, sort, ascending });
                        }
                        catch (Exception exc)
                        {
                            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                            return View(pregled);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Ne postoji osoba s tim identifikacijskim brojem.");
                        return View(pregled);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o pregledu nije moguće povezati s forme.");
                    return View(pregled);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;

                return RedirectToAction(nameof(Edit), new { page, sort, ascending });
            }
        }*/

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Pregled.AsNoTracking();

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
                return RedirectToAction(nameof(Index), new {
                    page = pagingInfo.TotalPages,
                    sort,
                    ascending
                });
            }

            System.Linq.Expressions.Expression<Func<Pregled, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.SifraPregleda;
                    break;
                case 2:
                    orderSelector = p => p.Datum;
                    break;
                case 3:
                    orderSelector = p => p.Anamneza;
                    break;
                case 4:
                    orderSelector = p => p.Dijagnoza;
                    break;

            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var pregledi = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new PreglediViewModel
            {
                Pregledi = pregledi,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pregled = await ctx.Pregled.FirstOrDefaultAsync(p => p.SifraPregleda == id);

            if (pregled == null)
            {
                return NotFound();
            }

            pregled.PregledSimptom = ctx.PregledSimptom.AsNoTracking()
                                                       .Where(p => p.SifraPregleda == id)
                                                       .ToList();

            List<Simptom> simptomi = new List<Simptom>();

            foreach(PregledSimptom ps in pregled.PregledSimptom)
            {
                var simptom = await ctx.Simptom.FirstOrDefaultAsync(p => p.SifraSimptoma == ps.SifraSimptoma);
                simptomi.Add(simptom);
            }

            pregled.PregledTerapija = ctx.PregledTerapija.AsNoTracking()
                                                       .Where(p => p.SifraPregleda == id)
                                                       .ToList();

            List<Terapija> terapije = new List<Terapija>();

            foreach (PregledTerapija pt in pregled.PregledTerapija)
            {
                var terapija = await ctx.Terapija.FirstOrDefaultAsync(p => p.SifraTerapije == pt.SifraTerapije);
                terapije.Add(terapija);
            }

            var osoba = await ctx.OsobaPregled.FirstOrDefaultAsync(p => p.SifraPregleda == id);

            string idOsoba = "";

            if (osoba == null)
            {
                idOsoba = "Nema ident. broj osobe";
            }
            else
            {
                idOsoba = osoba.IdentifikacijskiBroj;
            }

            var model = new PregledDetailViewModel
            {
                Pregled = pregled,
                Simptomi = simptomi,
                Terapije = terapije,
                IdOsoba = idOsoba
            };

            return View(model);
        }

        private decimal NewId()
        {
            var maxId = ctx.Pregled
                      .Select(o => o.SifraPregleda)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
    }
}
