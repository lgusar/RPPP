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
    public class LokacijaController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly AppSettings _appSettings;


        public LokacijaController(RPPP09Context context, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdownLists();
            return View();
        }

        private async Task PrepareDropdownLists()
        {
            var drzava = await _context.Drzava.OrderBy(d => d.ImeDrzave).Select(d => new { d.ImeDrzave, d.SifraDrzave }).ToListAsync();
            ViewBag.Drzave = new SelectList(drzava, nameof(Drzava.SifraDrzave), nameof(Drzava.ImeDrzave));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lokacija lokacija)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(lokacija);
                    _context.SaveChanges();
                    TempData[Constants.Message] = $"Grad {lokacija.SifraGrada} uspjesno dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.CompleteExceptionMessage());
                    await PrepareDropdownLists();
                    return View(lokacija);
                }
            }
            else
            {
                await PrepareDropdownLists();
                return View(lokacija);
            }
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;
            //var query = _context.Putovanje.AsNoTracking();
            var query = _context.Lokacija.Include(p => p.SifraDrzaveNavigation).AsNoTracking();
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

            System.Linq.Expressions.Expression<Func<Lokacija, object>> orderSelection = null;
            switch (sort)
            {
                case 1:
                    orderSelection = p => p.SifraGrada;
                    break;
                case 2:
                    orderSelection = p => p.ImeGrada;
                    break;
                case 3:
                    orderSelection = p => p.SifraDrzave;
                    break;
            }

            if (orderSelection != null)
            {
                query = ascending ? query.OrderBy(orderSelection) : query.OrderByDescending(orderSelection);
            }

            var lokacije = query.Skip((page - 1) * pagesize)
                                 .Take(_appSettings.PageSize)
                                 .ToList();
            var model = new LokacijaViewModel
            {
                Lokacija = lokacije,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var lokacija = _context.Lokacija
                .AsNoTracking()
                .Where(d => d.SifraGrada == id)
                .FirstOrDefault();

            if (lokacija == null)
            {
                return NotFound($"Ne postoji lokacija s tom šifrom: {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.ascending = ascending;
                await PrepareDropdownLists();
                return View(lokacija);
            }
        }

        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Lokacija lokacija = await _context.Lokacija.FindAsync(id);
                if (lokacija == null)
                {
                    return NotFound($"Ne postoji lokacija s identifikacijskom oznakom {id}");
                }
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Lokacija>(lokacija, "", z => z.ImeGrada, z => z.SifraDrzave);
                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Podaci o putovanju uspješno su ažurirani.";
                        TempData[Constants.ErrorOccurred] = false;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        await PrepareDropdownLists();
                        return View(lokacija);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o gradu nije moguće povezati s forme");
                    await PrepareDropdownLists();
                    return View(lokacija);
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
        public IActionResult Delete(int SifraGrada, int page = 1, int sort = 1, bool ascending = true)
        {
            var lokacija = _context.Lokacija.Find(SifraGrada);
                                  
            if (lokacija == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    string naziv = lokacija.ImeGrada;
                    _context.Remove(lokacija);
                    _context.SaveChanges();
                    TempData[Constants.Message] = $"Grad {naziv} je uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception ex)
                {
                    TempData[Constants.Message] = $"Pogreska prilikom brisanja grada: " + ex.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }

        }
    }
}