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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;

namespace KoronavirusMvc.Controllers
{
    /// <summary>
    /// Razred za backend rad sa simptomima
    /// Napravio Lovre Gusar
    /// </summary>
    public class SimptomController : Controller
    {
        private readonly RPPP09Context ctx;

        private readonly AppSettings appSettings;

        private readonly ILogger<SimptomController> logger;

        /// <summary>
        /// Konstruktor razreda SimptomController
        /// </summary>
        /// <param name="ctx">kontekst baze</param>
        /// <param name="optionsSnapshot">opcije</param>
        /// <param name="logger">logger za ispis logova prilikom unosa, brisanja i ažuriranja u bazi podataka</param>
        public SimptomController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<SimptomController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        /// <summary>
        /// Metoda za tablični ispis svih simptoma
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Simptom.AsNoTracking();

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
                return RedirectToAction(nameof(Index), new
                {
                    page = pagingInfo.TotalPages,
                    sort,
                    ascending
                });
            }

            System.Linq.Expressions.Expression<Func<Simptom, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = s => s.SifraSimptoma;
                    break;
                case 2:
                    orderSelector = s => s.Opis;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var simptomi = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new SimptomiViewModel
            {
                Simptomi = simptomi,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Metoda za dohvaćanje stranice Create.cshtml za stvaranje novog simptoma
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Metoda za stvaranje novog simptoma u bazi podataka
        /// </summary>
        /// <param name="simptom">Model simptom</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Simptom simptom)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    simptom.SifraSimptoma = (int)NewId();
                    ctx.Add(simptom);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Simptom {simptom.SifraSimptoma} uspješno dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    logger.LogInformation($"Simptom {simptom.SifraSimptoma} uspješno dodan.");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    logger.LogError($"Pogreška prilikom dodavanja novog simptoma {exc.CompleteExceptionMessage()}");
                    return View(simptom);
                }
            }
            else
            {
                logger.LogError($"Pogreška prilikom dodavanja novog simptoma");
                return View(simptom);
            }
        }

        /// <summary>
        /// Metoda za brisanje simptoma iz baze podataka
        /// </summary>
        /// <param name="SifraSimptoma">Šifra simptoma kojeg želimo izbrisati</param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int SifraSimptoma, int page = 1, int sort = 1, bool ascending = true)
        {
            var simptom = ctx.Simptom.Find(SifraSimptoma);
            if (simptom == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    ctx.Remove(simptom);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Simptom {simptom.SifraSimptoma} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;

                    logger.LogInformation($"Simptom {simptom.SifraSimptoma} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja simptoma." + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;

                    logger.LogError($"Pogreška prilikom brisanja simptoma. {exc.CompleteExceptionMessage()}");
                }
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        /// <summary>
        /// Metoda za dohvaćanje stranice Edit.cshtml za uređivanje simptoma
        /// </summary>
        /// <param name="id">Šifra simptoma koji želimo urediti</param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var simptom = ctx.Simptom
                             .AsNoTracking()
                             .Where(p => p.SifraSimptoma == id)
                             .FirstOrDefault();

            if (simptom == null)
            { 
                return NotFound($"Ne postoji simptom s tom šifrom: {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.ascending = ascending;
                return View(simptom);
            }
        }

        /// <summary>
        /// Metoda za uređivanje simptoma u bazi podataka
        /// </summary>
        /// <param name="id">Šifra simptoma koji želimo urediti</param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Simptom simptom = await ctx.Simptom.FindAsync(id);

                if (simptom == null)
                {
                    logger.LogError($"Pogreška prilikom ažuriranja simptoma. Ne postoji simptom s tom šifrom: {id}");
                    return NotFound($"Ne postoji simptom s tom šifrom {id}");
                }

                ViewBag.page = page;
                ViewBag.sort = sort;
                ViewBag.ascending = ascending;
                bool ok = await TryUpdateModelAsync<Simptom>(simptom, "", p => p.Opis);

                if (ok)
                {
                    try
                    {
                        TempData[Constants.Message] = $"Simptom {simptom.SifraSimptoma} uspješno ažuriran.";
                        TempData[Constants.ErrorOccurred] = false;

                        await ctx.SaveChangesAsync();

                        logger.LogInformation($"Simptom {simptom.SifraSimptoma} uspješno ažuriran.");

                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        logger.LogError($"Pogreška prilikom ažuriranja simptoma. {exc.CompleteExceptionMessage()}");
                        return View(simptom);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o simptomu nije moguće povezati s forme.");
                    logger.LogError($"Pogreška prilikom ažuriranja simptoma. Podatke o simptomu nije moguće povezati s forme.");
                    return View(simptom);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;

                logger.LogError($"Pogreška prilikom ažuriranja simptoma. {exc.CompleteExceptionMessage()}");

                return RedirectToAction(nameof(Edit), new { page, sort, ascending });
            }
        }

        /// <summary>
        /// Metoda za generiranje izvješća za Excel. Stvara se excel tablica sa svim simptomima
        /// </summary>
        public void ExportToExcel()
        {
            List<SimptomExcelViewModel> lista = ctx.Simptom.Select(s => new SimptomExcelViewModel
            {
                SifraSimptoma = s.SifraSimptoma,
                Opis = s.Opis
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Simptomi");

            ws.Cells["A1"].Value = "Simptomi";

            ws.Cells["A3"].Value = "Datum";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra simptoma";
            ws.Cells["B6"].Value = "Opis";

            int rowStart = 7;
            foreach (var item in lista)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraSimptoma;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Opis;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=simptomi.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();
        }

        /// <summary>
        /// Metoda za generiranje izvješća u pdf formatu. Stvara se tablica sa svim simptomima
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis simptoma";
            var simptomi = await ctx.Simptom
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(simptomi));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Simptom>(o => o.SifraSimptoma);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra simptoma", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Simptom>(o => o.Opis);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Left);
                });
            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=simptomi.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Pomoćna funkcija za generiranje nove šifre simptoma kad se stvara novi simptom. Novi simptom dobiva prvi najveći slobodan broj.
        /// </summary>
        /// <returns>Šifra novog simptoma</returns>
        private decimal NewId()
        {
            var maxId = ctx.Simptom
                      .Select(o => o.SifraSimptoma)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
    }
}
