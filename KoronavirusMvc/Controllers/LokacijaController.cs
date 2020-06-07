using System;
using System.Collections.Generic;
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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Lokacija lokacija)
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
                    return View(lokacija);
                }
            }
            else
            {
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