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
    public class StatistikaController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly AppSettings _appSettings;


        public StatistikaController(RPPP09Context context, IOptionsSnapshot<AppSettings> appSettings)
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
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;
            //var query = _context.Putovanje.AsNoTracking();
            var query = _context.Statistika.Include(p => p.SifraOrganizacijeNavigation).Include(s => s.SifraGradaNavigation).AsNoTracking();
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