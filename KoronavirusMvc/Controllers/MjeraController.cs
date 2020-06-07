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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoronavirusMvc.Controllers
{
    public class MjeraController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<MjeraController> logger;

        public MjeraController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<MjeraController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }


        [HttpGet]
        public IActionResult Create()
        {
            PrepareDropDownLists();
            return View();
        }

        private void PrepareDropDownLists()
        {
            var sastanci = ctx.Sastanak
                            .OrderBy(d => d.SifraSastanka)
                            .Select(d => new
                            {
                                d.SifraSastanka,
                            })
                            .ToList();
            ViewBag.Sastanci = new SelectList(sastanci, nameof(Sastanak.SifraSastanka), nameof(Sastanak.SifraSastanka));

            var mjere = ctx.Mjera
                .OrderBy(d => d.SifraMjere)
                .Select(d => new
                {
                    d.SifraMjere,
                })
                .ToList();
            ViewBag.Mjere = new SelectList(mjere, nameof(Mjera.SifraMjere), nameof(Mjera.SifraMjere));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Mjera mjera)
        {
            logger.LogTrace(JsonSerializer.Serialize(mjera), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(mjera);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(3000), $"Mjera {mjera.SifraMjere} dodana.");
                    TempData[Constants.Message] = $"Mjera {mjera.SifraMjere} dodana.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje nove mjere: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    PrepareDropDownLists();
                    return View(mjera);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(mjera);
            }
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            Mjera mjera = null;
            try
            {
                 mjera = ctx.Mjera
                .AsNoTracking()
                .Where(m => m.SifraMjere == id)
                .SingleOrDefault();
            } catch
            {
                return NotFound("Ova mjera se namjerno ne može promijeniti");
            }

            if (mjera != null)
            {
                PrepareDropDownLists();
                return PartialView(mjera);
            }
            else
            {
                return NotFound($"Neispravna šifra mjere: {id}");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Mjera mjera)
        {
            if (mjera == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Mjera.Any(m => m.SifraMjere == mjera.SifraMjere);
            if (!checkId)
            {
                return NotFound($"Neispravna šifra mjere: {mjera?.SifraMjere}");
            }

            PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(mjera);
                    ctx.SaveChanges();
                    return StatusCode(302, Url.Action(nameof(Row), new { id = mjera.SifraMjere }));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(mjera);
                }
            }
            else
            {
                return PartialView(mjera);
            }
        }

        public PartialViewResult Row(int id)
        {
            var mjera = ctx.Mjera
                             .Where(m => m.SifraMjere == id)
                             .Select(m => new MjeraViewModel
                             {
                                 SifraMjere = m.SifraMjere,
                                 SifraSastanka = m.SifraSastanka,
                                 SifraPrethodneMjere = m.SifraPrethodneMjere,
                                 Opis = m.Opis,
                                 Datum = m.Datum,
                                 VrijediDo = m.VrijediDo
                             })
                             .SingleOrDefault();
            if (mjera != null)
            {
                return PartialView(mjera);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan id mjere: {id}");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var mjera = ctx.Mjera
                             .AsNoTracking()
                             .Where(m => m.SifraMjere == id)
                             .SingleOrDefault();
            if (mjera != null)
            {
                try
                {
                    ctx.Remove(mjera);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Mjera sa šifrom {id} obrisana.",
                        successful = true
                    };
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja mjere: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Mjera sa šifrom {id} ne postoji");
            }
        }


        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Mjera.AsNoTracking();

            int count = query.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (page > pagingInfo.TotalItems)
            {
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages, sort, ascending });
            }

            System.Linq.Expressions.Expression<Func<Mjera, object>> orderSelector = null;

            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraMjere;
                    break;
                case 2:
                    orderSelector = d => d.SifraSastanka;
                    break;
                case 3:
                    orderSelector = d => d.SifraPrethodneMjere;
                    break;
                case 4:
                    orderSelector = d => d.Opis;
                    break;
                case 5:
                    orderSelector = d => d.Datum;
                    break;
                case 6:
                    orderSelector = d => d.VrijediDo;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var mjere = query
                      .Select(m => new MjeraViewModel
                      {
                          SifraMjere = m.SifraMjere,
                          SifraSastanka = m.SifraSastanka,
                          SifraPrethodneMjere = m.SifraPrethodneMjere == null ? -1 : m.SifraPrethodneMjere,
                          Opis = m.Opis,
                          Datum = m.Datum,
                          VrijediDo = m.VrijediDo
                      })
                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();


            var model = new MjereViewModel
            {
                Mjere = mjere,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public void ExportToExcel()
        {
            List<MjeraViewModel> emplist = ctx.Mjera.Select(x => new MjeraViewModel
            {
                SifraMjere = x.SifraMjere,
                SifraSastanka = x.SifraSastanka,
                SifraPrethodneMjere = x.SifraPrethodneMjere,
                Opis = x.Opis,
                Datum = x.Datum,
                VrijediDo = x.VrijediDo
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Mjere");

            ws.Cells["A1"].Value = "Mjere";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra mjere";
            ws.Cells["B6"].Value = "Sifra sastanka";
            ws.Cells["C6"].Value = "Sifra prethodne mjere";
            ws.Cells["D6"].Value = "Opis";
            ws.Cells["E6"].Value = "Datum";
            ws.Cells["F6"].Value = "Vrijedi do";


            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraMjere;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.SifraSastanka;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.SifraPrethodneMjere;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.Opis.Trim();
                ws.Cells[string.Format("E{0}", rowStart)].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", item.Datum);
                ws.Cells[string.Format("F{0}", rowStart)].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", item.VrijediDo);
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=mjere.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();
        }

        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis mjera";
            var mjere = await ctx.Mjera
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(mjere));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Mjera>(o => o.SifraMjere);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra mjere", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Mjera>(o => o.SifraPrethodneMjere);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Sifra prethodne mjere", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Mjera>(o => o.SifraSastanka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Sifra sastanka", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Mjera>(o => o.Opis);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Mjera>(o => o.Datum);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Datum", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Mjera>(o => o.VrijediDo);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Vrijedi do", horizontalAlignment: HorizontalAlignment.Left);
                });
            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=mjere.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }


    }
}