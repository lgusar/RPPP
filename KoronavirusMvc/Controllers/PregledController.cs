using System;
using System.Collections.Generic;
using System.Globalization;
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
            prepareDropDownTerapije();
            prepareDropDownSimptomi();
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
                    if(ctx.Osoba.Find(pregledCreate.OsobaPregled.IdentifikacijskiBroj) != null)
                    {
                        pregledCreate.Pregled.SifraPregleda = (int)NewId();
                        ctx.Add(pregledCreate.Pregled);
                        pregledCreate.OsobaPregled.SifraPregleda = pregledCreate.Pregled.SifraPregleda;
                        ctx.Add(pregledCreate.OsobaPregled);
                        
                        foreach(var opis in pregledCreate.Simptomi)
                        {
                            var simptom = ctx.Simptom.AsNoTracking().Where(p => p.Opis == opis).FirstOrDefault();

                            if (simptom != null)
                            {
                                ctx.Add(new PregledSimptom
                                {
                                    SifraPregleda = pregledCreate.Pregled.SifraPregleda,
                                    SifraSimptoma = simptom.SifraSimptoma
                                });
                            }
                        }

                        foreach (var opis in pregledCreate.Terapije)
                        {
                            var terapija = ctx.Terapija.AsNoTracking().Where(p => p.OpisTerapije == opis).FirstOrDefault();

                            if (terapija != null)
                            {
                                ctx.Add(new PregledTerapija
                                {
                                    SifraPregleda = pregledCreate.Pregled.SifraPregleda,
                                    SifraTerapije = terapija.SifraTerapije
                                });
                            }
                        }

                        ctx.SaveChanges();

                        TempData[Constants.Message] = $"Pregled {pregledCreate.Pregled.SifraPregleda} uspješno dodan.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index));
                    }

                    else
                    {
                        TempData[Constants.Message] = $"Ne postoji osoba s tim identifikacijskim brojem.";
                        TempData[Constants.ErrorOccurred] = true;
                        prepareDropDownTerapije();
                        prepareDropDownSimptomi();
                        return View(pregledCreate);
                    }
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    prepareDropDownTerapije();
                    prepareDropDownSimptomi();
                    return View(pregledCreate);
                }
            }
            else
            {
                prepareDropDownTerapije();
                prepareDropDownSimptomi();
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


                return View(new PregledCreateViewModel { 
                    Pregled = pregled,
                    OsobaPregled = idOsoba
                });
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Pregled pregled = await ctx.Pregled.FindAsync(id);
                OsobaPregled osobaPregled = await ctx.OsobaPregled.FindAsync(id);

                PregledCreateViewModel pc = new PregledCreateViewModel
                {
                    Pregled = pregled,
                    OsobaPregled = osobaPregled
                };

                if (pregled == null)
                {
                    return NotFound($"Ne postoji pregled s tom šifrom {id}");
                }

                ViewBag.page = page;
                ViewBag.sort = sort;
                ViewBag.ascending = ascending;
                bool ok = await TryUpdateModelAsync<PregledCreateViewModel>(pc, "", p => p.Pregled, p => p.OsobaPregled);

                if (ok)
                {
                    if (ctx.Osoba.Find(osobaPregled.IdentifikacijskiBroj) != null)
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
                            return View(pc);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Ne postoji osoba s tim identifikacijskim brojem.");
                        return View(pc);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o pregledu nije moguće povezati s forme.");
                    return View(pc);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;

                return RedirectToAction(nameof(Edit), new { page, sort, ascending });
            }
        }

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

        private void prepareDropDownSimptomi()
        {
            var simptomi = ctx.Simptom.AsNoTracking().ToList();
            List<string> opisi = new List<string>();
            foreach(var simptom in simptomi)
            {
                opisi.Add(simptom.Opis);
            }
            ViewBag.Simptomi = new MultiSelectList(opisi);
        }

        private void prepareDropDownTerapije()
        {
            var terapije = ctx.Terapija.AsNoTracking().ToList();
            List<string> opisi= new List<string>();
            foreach (var terapija in terapije)
            {
                opisi.Add(terapija.OpisTerapije);
            }
            ViewBag.Terapije = new MultiSelectList(opisi);
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
