using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Controllers
{
    public class DrzavaController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly AppSettings _appSettings;

        public DrzavaController(RPPP09Context context, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        // GET: Drzava/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Drzava/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Drzava drzava)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(drzava);
                    _context.SaveChanges();
                    TempData[Constants.Message] = $"Putovanje {drzava.SifraDrzave} uspjesno dodano.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.CompleteExceptionMessage());
                    return View(drzava);
                }
            }
            else
            {
                return View(drzava);
            }
        }


        // GET: Drzava/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Drzava/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Drzava/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Drzava/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string SifraDrzave, int page = 1, int sort = 1, bool ascending = true)
        {
                var drzava = _context.Drzava.Find(SifraDrzave);
                if(drzava == null)
                {
                    return NotFound();
                }
                else
                {
                    try
                    {
                    string naziv = drzava.ImeDrzave;
                    _context.Remove(drzava);
                    _context.SaveChanges();
                    TempData[Constants.Message] = $"Država {naziv} je uspješno pobrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                    }
                    catch(Exception ex)
                    {
                    TempData[Constants.Message] = $"Pogreska prilikom brisanja drzave: " + ex.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    }
                return RedirectToAction(nameof(Index), new {page, sort, ascending });
                }

        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;
            //var query = _context.Putovanje.AsNoTracking();
            var query = _context.Drzava.AsNoTracking();
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

            System.Linq.Expressions.Expression<Func<Drzava, object>> orderSelection = null;
            switch (sort)
            {
                case 1:
                    orderSelection = d => d.SifraDrzave;
                    break;
                case 2:
                    orderSelection = d => d.ImeDrzave;
                    break;

            }

            if (orderSelection != null)
            {
                query = ascending ? query.OrderBy(orderSelection) : query.OrderByDescending(orderSelection);
            }

            var drzave = query.Skip((page - 1) * pagesize)
                                 .Take(_appSettings.PageSize)
                                 .ToList();
            var model = new DrzavaViewModel
            {
                Drzava = drzave,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
    }
}