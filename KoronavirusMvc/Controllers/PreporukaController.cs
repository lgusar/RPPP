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
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;

namespace KoronavirusMvc.Controllers
{
    /// <summary>
    /// Kontroler za preporuku
    /// </summary>
    public class PreporukaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<PreporukaController> logger;

        public PreporukaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<PreporukaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        private async Task PrepareDropDownLists()
        {
            var organizacije = await ctx.Organizacija.OrderBy(d => d.SifraOrganizacije).Select(d => new { d.SifraOrganizacije, d.Naziv }).ToListAsync();
            ViewBag.Organizacije = new SelectList(organizacije, nameof(Organizacija.SifraOrganizacije), nameof(Organizacija.Naziv));

            var preporuke = await ctx.Preporuka.OrderBy(d => d.SifraPreporuke).Select(d => new { d.SifraPreporuke, d.Opis }).ToListAsync();
            ViewBag.Preporuke = new SelectList(preporuke, nameof(Preporuka.SifraPreporuke), nameof(Preporuka.Opis));

            var stozeri = await ctx.Stozer.OrderBy(d => d.SifraStozera).Select(d => new { d.SifraStozera, d.Naziv }).ToListAsync();
            ViewBag.Stozeri = new SelectList(stozeri, nameof(Stozer.SifraStozera), nameof(Stozer.Naziv));


        }

        /// <summary>
        /// Stvaranje nove preporuke
        /// </summary>
        /// <param name="preporuka">Preporuka</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Preporuka preporuka)
        {
            logger.LogTrace(JsonSerializer.Serialize(preporuka), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    preporuka.SifraPreporuke = (int)NewId();
                    ctx.Add(preporuka);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Preporuka {preporuka.Opis} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation(new EventId(1000), $"Preporuka {preporuka.Opis} dodana");

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom dodavanja nove preporuke {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(preporuka);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(preporuka);
            }
        }
        /// <summary>
        /// Prikaz tablice za preporuke
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="sort">Sort</param>
        /// <param name="ascending">Ascending (true ili false)</param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Preporuka.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Preporuka, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Opis;
                    break;
                case 2:
                    orderSelector = d => d.SifraOrganizacijeNavigation.Naziv;
                    break;
                case 3:
                    orderSelector = d => d.VrijemeObjave;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);

            }

            var preporuke = ctx.Preporuka
                              .Select(m => new PreporukaViewModel
                              {
                                  Opis = m.Opis,
                                  SifraPreporuke = m.SifraPreporuke,
                                  NazivOrganizacije = m.SifraOrganizacijeNavigation.Naziv,
                                  VrijemeObjave = m.VrijemeObjave,
                                  NazivStozera =m.SifraStozeraNavigation.Naziv,
                                  OpisPrethodnePreporuke = m.SifraPrethodnePreporukeNavigation.Opis,
                              })
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new PreporukeViewModel
            {
                Preporuke = preporuke,
                PagingInfo = pagingInfo
            };
            return View(model);

        }

        /// <summary>
        /// Brisanje preporuke sa nekim id-em
        /// </summary>
        /// <param name="id">Sifra preporuke</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Delete(int id)
        {
            var preporuka = await ctx.Preporuka.FindAsync(id);
            logger.LogTrace(JsonSerializer.Serialize(preporuka), new JsonSerializerOptions { IgnoreNullValues = true });
            if (preporuka != null)
            {
                try
                {
                    string naziv = preporuka.Opis;
                    ctx.Remove(preporuka);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation(new EventId(1000), $"Preporuka {preporuka.Opis} obrisana");
                    var result = new
                    {
                        message = $"Preporuka {naziv} sa šifrom {id} obrisana",
                        successful = true
                    };

                    return Json(result);
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom brisanja preporuke {exc.CompleteExceptionMessage()}");
                    var result = new
                    {
                        message = $"Pogreška prilikom brisanja preporuke {exc.CompleteExceptionMessage()}",
                        successful = false
                    };
                    return Json(result);
                }

            }
            else
            {
                return NotFound($"Preporuka sa šifrom {id} ne postoji");
            }
        }
        /// <summary>
        /// Ažuriranje preporuke s nekim id-em
        /// </summary>
        /// <param name="id">Sifra preporuke</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var preporuka = ctx.Preporuka
                             .AsNoTracking()
                             .Where(m => m.SifraPreporuke == id)
                             .SingleOrDefault();
            if (preporuka != null)
            {
                await PrepareDropDownLists();
                return PartialView(preporuka);
            }
            else
            {
                return NotFound($"Neispravna šifra preporuke: {id}");
            }
        }
        /// <summary>
        /// Ažuriranje preporuke
        /// </summary>
        /// <param name="preporuka">Sifra preporuka</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Preporuka preporuka)
        {
            logger.LogTrace(JsonSerializer.Serialize(preporuka), new JsonSerializerOptions { IgnoreNullValues = true });
            if (preporuka == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Preporuka.Any(m => m.SifraPreporuke == preporuka.SifraPreporuke);
            if (!checkId)
            {
                return NotFound($"Neispravna sifra preporuke: {preporuka?.SifraPreporuke}");
            }

            await PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(preporuka);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Preporuka {preporuka.Opis} ažurirana");
                    return StatusCode(302, Url.Action(nameof(Row), new { id = preporuka.SifraPreporuke }));
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom ažuriranja preporuke {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(preporuka);
                }
            }
            else
            {
                return PartialView(preporuka);
            }
        }
        /// <summary>
        /// Prikaz preporuke s nekim id-em
        /// </summary>
        /// <param name="id">Sifra preporuke</param>
        /// <returns></returns>
        public PartialViewResult Row(int id)
        {
            var preporuka = ctx.Preporuka
                             .Where(m => m.SifraPreporuke == id)
                             .Select(m => new PreporukaViewModel
                             {
                                 SifraPreporuke = m.SifraPreporuke,
                                 Opis = m.Opis,
                                 NazivOrganizacije = m.SifraOrganizacijeNavigation.Naziv,
                                 NazivStozera = m.SifraStozeraNavigation.Naziv,
                                 OpisPrethodnePreporuke = m.SifraPrethodnePreporukeNavigation.Opis,
                                 VrijemeObjave = m.VrijemeObjave
                             })
                             .SingleOrDefault();
            if (preporuka != null)
            {
                return PartialView(preporuka);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan id preporuke: {id}");
            }
        }
        /// <summary>
        /// Sifra  za novostvorenu preporuku
        /// </summary>
        /// <returns></returns>
        private decimal NewId()
        {
            var maxId = ctx.Preporuka
                      .Select(o => o.SifraPreporuke)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
        /// <summary>
        /// Export u excel datoteku (xls)
        /// </summary>
        public void ExportToExcel()
        {
            List<PreporukaViewModel> emplist = ctx.Preporuka.Select(m => new PreporukaViewModel
            {
                SifraPreporuke = m.SifraPreporuke,
                Opis = m.Opis,
                NazivOrganizacije = m.SifraOrganizacijeNavigation.Naziv,
                NazivStozera = m.SifraStozeraNavigation.Naziv,
                OpisPrethodnePreporuke = m.SifraPrethodnePreporukeNavigation.Opis,
                VrijemeObjave = m.VrijemeObjave
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Oprema");

            ws.Cells["A1"].Value = "Oprema";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra preporuke";
            ws.Cells["B6"].Value = "Opis preporuke";
            ws.Cells["C6"].Value = "Organizacija";
            ws.Cells["D6"].Value = "Vrijeme objave";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraPreporuke;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Opis;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.NazivOrganizacije;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.VrijemeObjave.ToString();
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=Preporuke.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }
        /// <summary>
        /// Export u pdf datoteku
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis preporuka";
            var preporuke = await ctx.Preporuka
                .Include(o => o.SifraOrganizacijeNavigation)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(preporuke));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Preporuka>(o => o.SifraPreporuke);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra preporuke", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Preporuka>(o => o.SifraOrganizacijeNavigation.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Organizacija", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Preporuka>(o => o.VrijemeObjave);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Vrijeme objave", horizontalAlignment: HorizontalAlignment.Left);
                });


            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Preporuke.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

    }
}