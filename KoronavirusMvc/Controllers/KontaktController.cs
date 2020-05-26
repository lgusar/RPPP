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
    public class KontaktController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        public KontaktController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Kontakt kontakt)
        {
            if (ModelState.IsValid)
            {

                try
                { 
                    ctx.Add(kontakt);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Osoba {kontakt.IdOsoba} uspješno dodana.";
                    TempData[Constants.ErrorOccurred] = false;

                    Kontakt kontakt1 = new Kontakt { IdOsoba = kontakt.IdKontakt, IdKontakt = kontakt.IdOsoba };
                    ctx.Add(kontakt1);
                    ctx.SaveChanges();


                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(kontakt);
                }
            }
            else
            {
                return View(kontakt);
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Kontakt.Include(k => k.IdKontaktNavigation).Include(k => k.IdOsobaNavigation).AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Kontakt, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = k => k.IdOsoba;
                    break;
                case 2:
                    orderSelector = k => k.IdKontaktNavigation.IdentifikacijskiBroj;
                    break;
                case 3:
                    orderSelector = k => k.IdKontaktNavigation.Ime;
                    break;
                case 4:
                    orderSelector = k => k.IdKontaktNavigation.Prezime;
                    break;


            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var kontakti = query
                            .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new KontaktiViewModel
            {
                Kontakti = kontakti,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string idOsoba, string idKontakt, int page = 1, int sort = 1, bool ascending = true)
        {
            Kontakt kontakt = ctx.Kontakt.Where(k => k.IdOsoba == idOsoba && k.IdKontakt == idKontakt).FirstOrDefault();
            Kontakt kontakt2 = ctx.Kontakt.Where(k => k.IdOsoba == idKontakt && k.IdKontakt == idOsoba).FirstOrDefault();
            if (kontakt == null || kontakt2 == null)
            {
                return NotFound();
            }
            else
            {
                try
                {

                    ctx.Remove(kontakt);
                    ctx.SaveChanges();

                    ctx.Remove(kontakt2);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Osoba uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja osobe: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction("Index");
            }
        }
    }
}