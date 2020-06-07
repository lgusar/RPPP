using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;

namespace KoronavirusMvc.Controllers
{
    /// <summary>
    /// Oprema kontroler
    /// </summary>
    public class OpremaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<OpremaController> logger;
        /// <summary>
        /// Stvaranje kontrolera za opremu
        /// </summary>
        /// <param name="ctx">Pokretanje baze</param>
        /// <param name="optionsSnapshot">Izgled stranice</param>
        /// <param name="logger">Stvaranje loggera</param>
        public OpremaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<OpremaController> logger)
        {
            this.logger = logger;
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
        }
        /// <summary>
        /// Stvaranje dropdown izbora
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        private async Task PrepareDropDownLists()
        {
            var opreme = await ctx.Institucija.OrderBy(d => d.SifraInstitucije).Select(d => new { d.NazivInstitucije, d.SifraInstitucije }).ToListAsync();
            ViewBag.Institucije = new SelectList(opreme, nameof(Institucija.SifraInstitucije), nameof(Institucija.NazivInstitucije));
        }
        /// <summary>
        /// Stvaranje nove opreme
        /// </summary>
        /// <param name="oprema">Sifra opreme</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Oprema oprema)
        {
            logger.LogTrace(JsonSerializer.Serialize(oprema), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    oprema.SifraOpreme = (int)NewId();
                    ctx.Add(oprema);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Oprema {oprema.NazivOpreme} sa šifrom {oprema.SifraOpreme} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation(new EventId(1000), $"Oprema {oprema.NazivOpreme} dodana");
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom dodavanja nove opreme {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(oprema);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(oprema);
            }
        }
        /// <summary>
        /// Pocetna stranice za pregled opreme
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="sort">Sort</param>
        /// <param name="ascending">Ascending (true ili false)</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Oprema.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Oprema, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraInstitucijeNavigation.NazivInstitucije;
                    break;
                case 2:
                    orderSelector = d => d.NazivOpreme;
                    break;
                case 3:
                    orderSelector = d => d.KolicinaOpreme;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var opreme = query
                              .Select(m => new OpremaViewModel
                              {
                                  SifraOpreme = m.SifraOpreme,
                                  NazivInstitucije = m.SifraInstitucijeNavigation.NazivInstitucije,
                                  NazivOpreme = m.NazivOpreme,
                                  KolicinaOpreme = m.KolicinaOpreme
                              })
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new OpremeViewModel
            {
                Opremas = opreme,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
        /// <summary>
        /// Brisanje opreme sa određenim id-om
        /// </summary>
        /// <param name="id">Sifra opreme</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete(int id)
        {
            
            var oprema = await ctx.Oprema.FindAsync(id);
            logger.LogTrace(JsonSerializer.Serialize(oprema), new JsonSerializerOptions { IgnoreNullValues = true });
            if (oprema != null)
            {
                try
                {
                    string naziv = oprema.NazivOpreme;
                    ctx.Remove(oprema);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation(new EventId(1000), $"Oprema {oprema.NazivOpreme} obrisana");
                    var result = new
                    {
                        message = $"Oprema {naziv} sa šifrom {id} obrisana",
                        successful = true
                    };

                    return Json(result);
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom brisanja opreme {exc.CompleteExceptionMessage()}");
                    var result = new
                    {
                        message = $"Pogreška prilikom brisanja opreme {exc.CompleteExceptionMessage()}",
                        successful = false
                    };
                    return Json(result);
                }

            }
            else
            {
                return NotFound($"Oprema sa šifrom {id} ne postoji");
            }
        }
        /// <summary>
        /// Ažuriranje opreme sa nekim id-em
        /// </summary>
        /// <param name="id">Sifra opreme</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var oprema = ctx.Oprema
                             .AsNoTracking()
                             .Where(m => m.SifraOpreme == id)
                             .SingleOrDefault();
            if (oprema != null)
            {
                await PrepareDropDownLists();
                return PartialView(oprema);
            }
            else
            {
                return NotFound($"Neispravna sifra opreme: {id}");
            }
        }
        /// <summary>
        /// Ažuriranje opreme
        /// </summary>
        /// <param name="oprema">Oprema</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Oprema oprema)
        {
            logger.LogTrace(JsonSerializer.Serialize(oprema), new JsonSerializerOptions { IgnoreNullValues = true });
            if (oprema == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Oprema.Any(m => m.SifraOpreme == oprema.SifraOpreme);
            if (!checkId)
            {
                return NotFound($"Neispravna sifra institucije: {oprema?.SifraOpreme}");
            }

            await PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(oprema);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Oprema {oprema.NazivOpreme} ažurirana");
                    return StatusCode(302, Url.Action(nameof(Row), new { id = oprema.SifraOpreme }));
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom brisanja opreme {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(oprema);
                }
            }
            else
            {
                return PartialView(oprema);
            }
        }
        /// <summary>
        /// Prikaz atributa opreme sa određenim id-om
        /// </summary>
        /// <param name="id">Sifra opreme</param>
        /// <returns></returns>
        public PartialViewResult Row(int id)
        {
            var mjesto = ctx.Oprema
                             .Where(m => m.SifraOpreme == id)
                             .Select(m => new OpremaViewModel
                             {
                                 SifraOpreme = m.SifraOpreme,
                                 NazivInstitucije = m.SifraInstitucijeNavigation.NazivInstitucije,
                                 NazivOpreme = m.NazivOpreme,
                                 KolicinaOpreme = m.KolicinaOpreme
                             })
                             .SingleOrDefault();
            if (mjesto != null)
            {
                return PartialView(mjesto);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan id opreme: {id}");
            }
        }
        /// <summary>
        /// Stvaranje sifre novonastale opreme
        /// </summary>
        /// <returns></returns>
        private decimal NewId()
        {
            var maxId = ctx.Oprema
                      .Select(o => o.SifraOpreme)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
        /// <summary>
        /// Export u excel datoteku
        /// </summary>
        public void ExportToExcel()
        {
            List<OpremaViewModel> emplist =ctx.Oprema.Select(x => new OpremaViewModel
            {
                SifraOpreme = x.SifraOpreme,
                NazivInstitucije = x.SifraInstitucijeNavigation.NazivInstitucije,
                NazivOpreme = x.NazivOpreme,
                KolicinaOpreme = x.KolicinaOpreme
            }).ToList();
            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Oprema");

            ws.Cells["A1"].Value = "Oprema";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra Opreme";
            ws.Cells["B6"].Value = "Naziv Institucije";
            ws.Cells["C6"].Value = "Naziv Opreme";
            ws.Cells["D6"].Value = "Kolicina opreme";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraOpreme;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.NazivInstitucije;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.NazivOpreme;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.KolicinaOpreme;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=Oprema.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }
        /// <summary>
        /// Export u pdf datoteku
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis opreme";
            var opreme = await ctx.Oprema
                .Include(o => o.SifraInstitucijeNavigation)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(opreme));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Oprema>(o => o.SifraOpreme);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra opreme", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Oprema>(o => o.SifraInstitucijeNavigation.NazivInstitucije);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Naziv institucije", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Oprema>(o => o.NazivOpreme);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Naziv opreme", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Oprema>(o => o.KolicinaOpreme);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Kolicina opreme", horizontalAlignment: HorizontalAlignment.Left);
                });
               

            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Oprema.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

    }
}