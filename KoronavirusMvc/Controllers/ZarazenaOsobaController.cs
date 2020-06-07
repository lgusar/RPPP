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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;

namespace KoronavirusMvc.Controllers
{
    public class ZarazenaOsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<ZarazenaOsobaController> logger;
        public ZarazenaOsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<ZarazenaOsobaController> logger)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            PrepareDropDownLists();
            return View();
        }

        private void PrepareDropDownLists()
        {
            var stanja = ctx.Stanje.OrderBy(s => s.NazivStanja).Select(s => new { s.NazivStanja, s.SifraStanja }).ToList();
            ViewBag.Stanja = new SelectList(stanja, nameof(Stanje.SifraStanja), nameof(Stanje.NazivStanja));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ZarazenaOsoba zarazenaOsoba)
        {
            logger.LogTrace(JsonSerializer.Serialize(zarazenaOsoba));
            if (ModelState.IsValid)
            {

                try
                {
                    ctx.Add(zarazenaOsoba);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Osoba {zarazenaOsoba.IdentifikacijskiBroj} uspješno dodana u listu zaraženih osoba. ";
                    logger.LogInformation($"Osoba dodana");
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    logger.LogError($"Pogreška prilikom dodavanja zaražene osobe {exc.CompleteExceptionMessage()}");
                    PrepareDropDownLists();
                    return View(zarazenaOsoba);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(zarazenaOsoba);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                             .Include(o => o.IdentifikacijskiBrojNavigation)
                             .AsNoTracking()
                             .Where(m => m.IdentifikacijskiBroj == id)
                             .SingleOrDefault();
            if (zarazenaOsoba != null)
            {
                PrepareDropDownLists();
                return PartialView(zarazenaOsoba);
            }
            else
            {
                return NotFound($"Neispravan id osobe: {id}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ZarazenaOsoba zarazenaOsoba)
        {
            logger.LogTrace(JsonSerializer.Serialize(zarazenaOsoba));
            if (zarazenaOsoba == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.ZarazenaOsoba.Any(m => m.IdentifikacijskiBroj == zarazenaOsoba.IdentifikacijskiBroj);
            if (!checkId)
            {
                return NotFound($"Neispravan identifikacijski broj zarazene osobe: {zarazenaOsoba?.IdentifikacijskiBroj}");
            }

            PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(zarazenaOsoba);
                    ctx.SaveChanges();
                    logger.LogInformation($"Osoba ažurirana");
                    return StatusCode(302, Url.Action(nameof(Row), new { id = zarazenaOsoba.IdentifikacijskiBroj }));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    logger.LogError($"Pogreška prilikom ažuriranja zaražene osobe {exc.CompleteExceptionMessage()}");
                    return PartialView(zarazenaOsoba);
                }
            }
            else
            {
                return PartialView(zarazenaOsoba);
            }
        }

        public PartialViewResult Row(string id)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                                    .Where(z => z.IdentifikacijskiBroj == id)
                                    .Select(z => new ZarazenaOsobaViewModel
                                    {
                                        IdentifikacijskiBroj = z.IdentifikacijskiBroj,
                                        Ime = z.IdentifikacijskiBrojNavigation.Ime,
                                        Prezime = z.IdentifikacijskiBrojNavigation.Prezime,
                                        DatZaraze = z.DatZaraze,
                                        NazivStanja = z.SifraStanjaNavigation.NazivStanja
                                    })
                                    .SingleOrDefault();
            if(zarazenaOsoba != null)
            {
                return PartialView(zarazenaOsoba);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan identifikacijski broj osobe.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var zarazenaOsoba = ctx.ZarazenaOsoba
                             .AsNoTracking() 
                             .Where(m => m.IdentifikacijskiBroj == id)
                             .SingleOrDefault();
            logger.LogTrace(JsonSerializer.Serialize(zarazenaOsoba));
            if (zarazenaOsoba != null)
            {
                try
                {
                    
                    ctx.Remove(zarazenaOsoba);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Zaražena osoba obrisana.",
                        successful = true
                    };
                    logger.LogInformation($"Osoba obrisana");
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja zaražene osobe: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    logger.LogError($"Pogreška prilikom brisanja zaražene osobe {exc.CompleteExceptionMessage()}");
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Zaražena osoba s identifikacijskim brojem {id} ne postoji");
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.ZarazenaOsoba.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<ZarazenaOsoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = z => z.IdentifikacijskiBroj;
                    break;
                case 2:
                    orderSelector = z => z.IdentifikacijskiBrojNavigation.Ime;
                    break;
                case 3:
                    orderSelector = z => z.IdentifikacijskiBrojNavigation.Prezime;
                    break;
                case 4:
                    orderSelector = z => z.DatZaraze;
                    break;
                case 5:
                    orderSelector = z => z.SifraStanjaNavigation.NazivStanja;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var zarazeneOsobe = query
                            .Select(z => new ZarazenaOsobaViewModel
                            {
                                IdentifikacijskiBroj = z.IdentifikacijskiBroj,
                                Ime = z.IdentifikacijskiBrojNavigation.Ime,
                                Prezime = z.IdentifikacijskiBrojNavigation.Prezime,
                                DatZaraze = z.DatZaraze,
                                NazivStanja = z.SifraStanjaNavigation.NazivStanja
                            })
                           .ToList();
            var model = new ZarazeneOsobeViewModel
            {
                ZarazeneOsobe = zarazeneOsobe,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis zaraženih osoba";
            var zarazeneOsobe = await ctx.ZarazenaOsoba
                .Include(o => o.IdentifikacijskiBrojNavigation)
                .Include(o => o.SifraStanjaNavigation)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zarazeneOsobe));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<ZarazenaOsoba>(o => o.IdentifikacijskiBroj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Identifikacijski broj", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<ZarazenaOsoba>(o => o.IdentifikacijskiBrojNavigation.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<ZarazenaOsoba>(o => o.IdentifikacijskiBrojNavigation.Prezime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<ZarazenaOsoba>(o => o.SifraStanjaNavigation.NazivStanja);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Stanje osobe", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<ZarazenaOsoba>(o => o.DatZaraze);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Datum zaraze", horizontalAlignment: HorizontalAlignment.Center);
                    column.ColumnItemsTemplate(template =>
                    {
                        template.TextBlock();
                        template.DisplayFormatFormula(obj =>
                        {
                            if (obj == null || string.IsNullOrEmpty(obj.ToString()))
                            {
                                return string.Empty;
                            }
                            else
                            {
                                DateTime date = (DateTime)obj;
                                return date.ToString("dd.MM.yyyy");
                            }
                        });
                    });
                });

            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=zarazeneosobe.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public void ExportToExcel()
        {
            List<ZarazenaOsobaViewModel> emplist = ctx.ZarazenaOsoba.Select(x => new ZarazenaOsobaViewModel
            {
                IdentifikacijskiBroj = x.IdentifikacijskiBroj,
                Ime = x.IdentifikacijskiBrojNavigation.Ime,
                Prezime = x.IdentifikacijskiBrojNavigation.Prezime,
                DatZaraze = x.DatZaraze,
                NazivStanja = x.SifraStanjaNavigation.NazivStanja
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Zaražene osobe");

            ws.Cells["A1"].Value = "Zaražene osobe";

            ws.Cells["A3"].Value = "Datum";
            ws.Cells["B3"].Value = string.Format("{0:dd.MM.yyyy}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Identifikacijski broj osobe";
            ws.Cells["B6"].Value = "Ime";
            ws.Cells["C6"].Value = "Prezime";
            ws.Cells["D6"].Value = "Datum zaraze";
            ws.Cells["E6"].Value = "Stanje osobe";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.IdentifikacijskiBroj;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Ime;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.Prezime;
                ws.Cells[string.Format("D{0}", rowStart)].Value = string.Format("{0:dd.MM.yyyy}", item.DatZaraze);
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.NazivStanja;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=myfile.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }
    }
}
