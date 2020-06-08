using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KoronavirusMvc.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PdfRpt.FluentInterface;
using System.Collections.Generic;
using PdfRpt.Core.Contracts;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;

namespace KoronavirusMvc.Controllers
{
    /// <summary>
    /// Kontroler za sastanak
    /// </summary>
    public class SastanakController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<SastanakController> logger;

        /// <summary>
        /// Kreiranje kontrolera za sastanak
        /// </summary>
        /// <param name="ctx">Postavljanje baze</param>
        /// <param name="optionsSnapshot">Postavljanje postavki baze</param>
        /// <param name="logger">Postavljanje logera</param>
        public SastanakController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<SastanakController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        /// <summary>
        /// Stvaranje novog pogleda i DropDownListe
        /// </summary>
        /// <returns>Stvoreni pogled</returns>
        [HttpGet]
        public IActionResult Create()
        {
            PrepareDropDownLists();
            return View();
        }


        private void PrepareDropDownLists()
        {
            var stozeri = ctx.Stozer
                            .OrderBy(d => d.SifraStozera)
                            .Select(d => new { d.Naziv, d.SifraStozera })
                            .ToList();
            ViewBag.Stozeri = new SelectList(stozeri, nameof(Stozer.SifraStozera), nameof(Stozer.Naziv));
        }

        /// <summary>
        /// Metoda koja sprema novi sastanak u bazu podataka
        /// </summary>
        /// <param name="sastanak">Model koji sadrži sve atribute tablice Sastanak za dodavanje u bazu podataka</param>
        /// <returns>Pogled</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sastanak sastanak)
        {
            logger.LogTrace(JsonSerializer.Serialize(sastanak), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(sastanak);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(2000), $"Sastanak {sastanak.SifraSastanka} dodan.");
                    TempData[Constants.Message] = $"Sastanak dodan. Šifra sastanka = {sastanak.SifraSastanka}";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog sastanka: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    PrepareDropDownLists();
                    return View(sastanak);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(sastanak);
            }
        }

        /// <summary>
        /// Azuriranje zeljenog sastanka
        /// </summary>
        /// <param name="id">ID sastanka kojeg zelimo azurirati</param>
        /// <returns>pogled ili pogresku ako sastanak nije pronadjen</returns>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sastanak = ctx.Sastanak
                             .AsNoTracking()
                             .Where(m => m.SifraSastanka == id)
                             .SingleOrDefault();
            if (sastanak != null)
            {
                PrepareDropDownLists();
                return PartialView(sastanak);
            }
            else
            {
                return NotFound($"Neispravna šifra sastanka: {id}");
            }
        }

        /// <summary>
        /// Azuriranje zeljenog sastanka
        /// </summary>
        /// <param name="sastanak">Zeljeni sastanak za azuriranje</param>
        /// <returns>pogled ili pogreska ako sastanak nije pronadjen</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sastanak sastanak)
        {
            if (sastanak == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Sastanak.Any(m => m.SifraSastanka == sastanak.SifraSastanka);
            if (!checkId)
            {
                return NotFound($"Neispravna šifra sastanka: {sastanak?.SifraSastanka}");
            }

            PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(sastanak);
                    ctx.SaveChanges();
                    return StatusCode(302, Url.Action(nameof(Row), new { id = sastanak.SifraSastanka }));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(sastanak);
                }
            }
            else
            {
                return PartialView(sastanak);
            }
        }

        /// <summary>
        /// Metoda za ispis jednog retka tablice Sastanak
        /// </summary>
        /// <param name="id">ID retka kojeg zelimo ispisati</param>
        /// <returns>novi pogled</returns>
        public PartialViewResult Row(int id)
        {
            var sastanak = ctx.Sastanak
                             .Where(m => m.SifraSastanka == id)
                             .Select(m => new SastanakViewModel
                             {
                                 NazivStozera = m.SifraStozeraNavigation.Naziv,
                                 Datum = m.Datum,
                                 SifraSastanka = m.SifraSastanka
                             })
                             .SingleOrDefault();
            if (sastanak != null)
            {
                return PartialView(sastanak);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan id sastanka: {id}");
            }
        }

        /// <summary>
        ///  Metoda za brisanje sastanka u bazi 
        /// </summary>
        /// <param name="id">ID sastanka kojeg zelimo obrisati</param>
        /// <returns>JSON rezultat</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var sastanak = ctx.Sastanak
                             .AsNoTracking() //ima utjecaj samo za Update, za brisanje možemo staviti AsNoTracking
                             .Where(m => m.SifraSastanka == id)
                             .SingleOrDefault();
            if (sastanak != null)
            {
                try
                {
                    ctx.Remove(sastanak);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Sastanak sa šifrom {id} obrisan.",
                        successful = true
                    };
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja sastanka: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Sastanak sa šifrom {id} ne postoji");
            }
        }

        /// <summary>
        /// Tablicni prikaz sastanaka
        /// </summary>
        /// <param name="page">Stranica koju zelimo prikazati</param>
        /// <param name="sort">Index stupca po kojem sortiramo</param>
        /// <param name="ascending">Smjer sortiranja (true za uzlazno)</param>
        /// <returns>novi pogled</returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Sastanak.Include(z => z.SifraStozeraNavigation).AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Sastanak, object>> orderSelector = null;

            switch (sort)
            {
                case 1:
                    orderSelector = d => d.SifraStozeraNavigation.Naziv;
                    break;
                case 3:
                    orderSelector = d => d.Datum;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var sastanci = query
                      .Select(m => new SastanakViewModel
                      {
                          NazivStozera = m.SifraStozeraNavigation.Naziv,
                          Datum = m.Datum,
                          SifraSastanka = m.SifraSastanka
                      })
                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();


            var model = new SastanciViewModel
            {
                Sastanci = sastanci,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Excel izvoz
        /// </summary>
        public void ExportToExcel()
        {
            List<SastanakViewModel> emplist = ctx.Sastanak.Select(x => new SastanakViewModel
            {
                SifraSastanka = x.SifraSastanka,
                Datum = x.Datum,
                NazivStozera = x.SifraStozeraNavigation.Naziv.Trim()
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sastanci");

            ws.Cells["A1"].Value = "Sastanci";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra sastanka";
            ws.Cells["B6"].Value = "Datum";
            ws.Cells["C6"].Value = "Naziv stozera";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraSastanka;
                ws.Cells[string.Format("B{0}", rowStart)].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", item.Datum);
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.NazivStozera;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=myfile.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }

        /// <summary>
        /// Kreiranje PDF izvjestaja
        /// </summary>
        /// <returns>izvjestaj u PDF formatu</returns>
        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis sastanaka";
            var sastanci = await ctx.Sastanak
                .Include(o => o.SifraStozeraNavigation)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(sastanci));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Sastanak>(o => o.SifraSastanka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra sastanka", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Sastanak>(o => o.Datum);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Datum sastanka", horizontalAlignment: HorizontalAlignment.Left);
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
                    columns.AddColumn(column =>
                    {
                        column.PropertyName<Sastanak>(o => o.SifraStozeraNavigation.Naziv);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(2);
                        column.Width(2);
                        column.HeaderCell("Naziv stozera", horizontalAlignment: HorizontalAlignment.Center);
                    });
                });
            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=stozeri.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }


    }
}