using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;

namespace KoronavirusMvc.Controllers
{
    public class StanjeController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<StanjeController> logger;
        public StanjeController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<StanjeController> logger)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Stanje stanje)
        {
            logger.LogTrace(JsonSerializer.Serialize(stanje));
            if (ModelState.IsValid)
            {

                try
                {
                    stanje.SifraStanja = (int)NewId();
                    ctx.Add(stanje);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Stanje {stanje.NazivStanja} uspješno dodano.";
                    logger.LogInformation($"Stanje dodano");
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    logger.LogError($"Pogreška prilikom dodavanja stanja {exc.CompleteExceptionMessage()}");
                    return View(stanje);
                }
            }
            else
            {
                return View(stanje);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var stanje = ctx.Stanje.Find(id);
            if (stanje == null)
            {
                return NotFound($"Ne postoji stanje sa šifrom {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(stanje);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Stanje stanje = await ctx.Stanje.FindAsync(id);
                logger.LogTrace(JsonSerializer.Serialize(stanje));
                if (stanje == null)
                {
                    return NotFound($"Ne postoji stanje sa šifrom {id}");
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Stanje>(stanje, "", s => s.NazivStanja);
                if (ok)
                {
                    try
                    {
                        
                        TempData[Constants.Message] = $"Podaci stanja {stanje.NazivStanja} uspješno ažurirani.";
                        logger.LogInformation($"Stanje ažurirano");
                        TempData[Constants.ErrorOccurred] = false;
                        await ctx.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        logger.LogError($"Pogreška prilikom ažuriranja stanja {exc.CompleteExceptionMessage()}");
                        return View(stanje);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o stanju nije moguće povezati s forme");
                    return View(stanje);
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
        public IActionResult Delete(int sifrastanja, int page = 1, int sort = 1, bool ascending = true)
        {
            var stanje = ctx.Stanje.Find(sifrastanja);
            logger.LogTrace(JsonSerializer.Serialize(stanje));
            if (stanje == null)
            {
                return NotFound();
            }
            else
            {
                try
                {

                    ctx.Remove(stanje);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Stanje uspješno obrisano.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation($"Stanje {sifrastanja} obrisano");
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja stanja: " + exc.CompleteExceptionMessage();
                    logger.LogError($"Pogreška prilikom brisanja stanja {exc.CompleteExceptionMessage()}");
                    TempData[Constants.ErrorOccurred] = true;
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }


        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Stanje.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Stanje, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = s => s.SifraStanja;
                    break;
                case 2:
                    orderSelector = s => s.NazivStanja;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var stanja = query
                            .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new StanjaViewModel
            {
                Stanja = stanja,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
        private decimal NewId()
        {
            var maxId = ctx.Stanje
                      .Select(o => o.SifraStanja)
                      .ToList()
                      .Max();

            return maxId + 1;
        }

        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis stanja";
            var stanja = await ctx.Stanje
                .AsNoTracking()
                .ToListAsync();
            PdfReport report = Constants.CreateBasicReport(naslov);
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(stanja));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Stanje>(o => o.SifraStanja);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Šifra stanja", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Stanje>(o => o.NazivStanja);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Naziv stanja", horizontalAlignment: HorizontalAlignment.Left);
                });
                
            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=putovanja.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }
    }
}