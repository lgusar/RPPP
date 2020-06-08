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
    public class StatistikaController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly AppSettings _appSettings;

        /// <summary>
        /// stvaranje konteksta
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appSettings"></param>
        public StatistikaController(RPPP09Context context, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }
        /// <summary>
        /// stvaranje statistike
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdownLists();
            return View();
        }
        /// <summary>
        /// generiranje padajucih lista sa stranim kljucevima
        /// </summary>
        /// <returns></returns>
        private async Task PrepareDropdownLists()
        {
            var grad = await _context.Lokacija.OrderBy(d => d.ImeGrada).Select(d => new { d.ImeGrada, d.SifraGrada }).ToListAsync();
            ViewBag.Gradovi = new SelectList(grad, nameof(Lokacija.SifraGrada), nameof(Lokacija.ImeGrada));
            var organizacija = await _context.Organizacija.OrderBy(d => d.Naziv).Select(d => new { d.Naziv, d.SifraOrganizacije }).ToListAsync();
            ViewBag.Organizacije = new SelectList(organizacija, nameof(Organizacija.SifraOrganizacije), nameof(Organizacija.Naziv));

        }

        /// <summary>
        /// stvaranje statistike
        /// </summary>
        /// <param name="statistika"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Statistika statistika)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(statistika);
                    _context.SaveChanges();
                    TempData[Constants.Message] = $"Statistika {statistika.SifraObjave} uspjesno dodano.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.CompleteExceptionMessage());
                    return View(statistika);
                }
            }
            else
            {
                return View(statistika);
            }
        }

        /// <summary>
        /// generiranje podataka za stranicenje
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <param name="cityCode"></param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true, int? cityCode = null)
        {
            int pagesize = _appSettings.PageSize;
            //var query = _context.Putovanje.AsNoTracking();
            var query = _context.Statistika.Include(p => p.SifraOrganizacijeNavigation).Include(s => s.SifraGradaNavigation).AsNoTracking();
            if (cityCode.HasValue)
            {
                query = query.Where(s => s.SifraGrada == cityCode.Value);
            }
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
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages, sort, ascending });
            }

            System.Linq.Expressions.Expression<Func<Statistika, object>> orderSelection = null;
            switch (sort)
            {
                case 1:
                    orderSelection = p => p.SifraObjave;
                    break;
                case 2:
                    orderSelection = p => p.SifraGrada;
                    break;
                case 3:
                    orderSelection = p => p.SifraOrganizacije;
                    break;
                case 4:
                    orderSelection = p => p.BrojSlucajeva;
                    break;
                case 5:
                    orderSelection = p => p.BrojAktivnih;
                    break;
                case 6:
                    orderSelection = p => p.BrojUmrlih;
                    break;
                case 7:
                    orderSelection = p => p.BrojIzlijecenih;
                    break;
            }

            if (orderSelection != null)
            {
                query = ascending ? query.OrderBy(orderSelection) : query.OrderByDescending(orderSelection);
            }

            var statistika = query.Skip((page - 1) * pagesize)
                                 .Take(_appSettings.PageSize)
                                 .ToList();
            var model = new StatistikaViewModel
            {
                Statistika = statistika,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        /// <summary>
        /// dobivanje podataka za uredenje
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var statistika = _context.Statistika
                .AsNoTracking()
                .Where(d => d.SifraObjave == id)
                .FirstOrDefault();

            if (statistika == null)
            {
                return NotFound($"Ne postoji statistika s tom šifrom: {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.ascending = ascending;
                await PrepareDropdownLists();
                return View(statistika);
            }
        }

        /// <summary>
        /// uredivanje postojecih podataka
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Statistika statistika = await _context.Statistika.FindAsync(id);
                if (statistika == null)
                {
                    return NotFound($"Ne postoji statistika s identifikacijskom oznakom {id}");
                }
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Statistika>(statistika, "", z => z.SifraGrada, z => z.SifraOrganizacije, z => z.BrojSlucajeva, 
                                                                            z => z.BrojUmrlih, z => z.BrojIzlijecenih, z => z.BrojAktivnih, z => z.Datum);
                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Podaci o statistici uspješno su ažurirani.";
                        TempData[Constants.ErrorOccurred] = false;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        await PrepareDropdownLists();
                        return View(statistika);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o statistici nije moguće povezati s forme");
                    await PrepareDropdownLists();
                    return View(statistika);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;

                return RedirectToAction(nameof(Edit), new { id, page, sort, ascending });
            }
        }


        /// <summary>
        /// brisanje podataka
        /// </summary>
        /// <param name="SifraObjave"></param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int SifraObjave, int page = 1, int sort = 1, bool ascending = true)
        {
            var statistika = _context.Statistika.Find(SifraObjave);

            if (statistika == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    int naziv = statistika.SifraObjave;
                    _context.Remove(statistika);
                    _context.SaveChanges();
                    TempData[Constants.Message] = $"Statistika broja {naziv} je uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception ex)
                {
                    TempData[Constants.Message] = $"Pogreska prilikom brisanja statistike: " + ex.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }

        }
    }
}