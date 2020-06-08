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
    /// <summary>
    /// Kontroler za stozer
    /// </summary>
    public class StozerController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<StozerController> logger;

        /// <summary>
        /// Izrada kontrolera za stozer
        /// </summary>
        /// <param name="ctx">Postavljanje baze</param>
        /// <param name="optionsSnapshot">Postavljanje postavki baze</param>
        /// <param name="logger">Postavljanje logera</param>
        public StozerController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<StozerController> logger)
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
            var osobe = ctx.Osoba
                            .OrderBy(d => d.IdentifikacijskiBroj)
                            .Select(d => new
                            {
                                IdentifikacijskiBroj = d.IdentifikacijskiBroj,
                                imePrezime = string.Format("{0} {1}", d.Ime, d.Prezime)
                            })
                            .ToList();
            ViewBag.Osobe = new SelectList(osobe, nameof(Osoba.IdentifikacijskiBroj), nameof(Osoba.imePrezime));
        }

        /// <summary>
        /// Metoda koja sprema novi stozer u bazu podataka
        /// </summary>
        /// <param name="stozer">Model koji sadrži sve atribute tablice Stozer za dodavanje u bazu podataka</param>
        /// <returns>Pogled</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Stozer stozer)
        {
            logger.LogTrace(JsonSerializer.Serialize(stozer), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(stozer);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Stožer {stozer.Naziv} dodan.");
                    TempData[Constants.Message] = $"Stožer {stozer.Naziv} dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog stožera: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    PrepareDropDownLists();
                    return View(stozer);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(stozer);
            }
        }

        /// <summary>
        /// Azuriranje zeljenog stozera
        /// </summary>
        /// <param name="id">ID stozera kojeg zelimo azurirati</param>
        /// <returns>novi pogled ilil pogreska ako stozer nije pronadjen</returns>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var stozer = ctx.Stozer
                             .AsNoTracking()
                             .Where(m => m.SifraStozera == id)
                             .SingleOrDefault();
            if (stozer != null)
            {
                PrepareDropDownLists();
                return PartialView(stozer);
            }
            else
            {
                return NotFound($"Neispravna šifra stožera: {id}");
            }
        }

        /// <summary>
        /// Azuriranje zeljenog stozera
        /// </summary>
        /// <param name="stozer">Stozer kojeg zelimo azurirati</param>
        /// <returns>novi pogled ili pogreska ako se stozer ne moze pronaci</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Stozer stozer)
        {
            if (stozer == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Stozer.Any(m => m.SifraStozera == stozer.SifraStozera);
            if (!checkId)
            {
                return NotFound($"Neispravna šifra stožera: {stozer?.SifraStozera}");
            }

            PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(stozer);
                    ctx.SaveChanges();
                    return StatusCode(302, Url.Action(nameof(Row), new { id = stozer.SifraStozera }));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(stozer);
                }
            }
            else
            {
                return PartialView(stozer);
            }
        }

        /// <summary>
        /// Metoda za azuriranje zeljenog sastanka
        /// </summary>
        /// <param name="id">ID zeljenog sastanka</param>
        /// <returns>novi pogled ili pogreska ako sastanak nije pronadjen</returns>
        [HttpGet]
        public IActionResult EditSastanak(int id)
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
        /// Metoda za azuriranje zeljenog sastanka
        /// </summary>
        /// <param name="sastanak">Sastanak kojeg zelimo azurirati</param>
        /// <returns>novi pogled ili pogreska</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSastanak(Sastanak sastanak)
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
                    return StatusCode(302, Url.Action(nameof(RowSastanak), new { id = sastanak.SifraSastanka }));
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
        /// Metoda za ispis jednog retka tablice stozer
        /// </summary>
        /// <param name="id">ID retka kojeg zelimo ispisati</param>
        /// <returns>novi pogled</returns>
        public PartialViewResult Row(int id)
        {
            var stozer = ctx.Stozer
                             .Where(m => m.SifraStozera == id)
                             .Select(m => new StozerViewModel
                             {
                                 ImePredsjednika = m.IdPredsjednikaNavigation.Ime + m.IdPredsjednikaNavigation.Prezime,
                                 SifraStozera = m.SifraStozera,
                                 Naziv = m.Naziv
                             })
                             .SingleOrDefault();
            if (stozer != null)
            {
                return PartialView(stozer);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan id stozera: {id}");
            }
        }

        /// <summary>
        /// Metoda za ispis jednog retka tablice Sastanak
        /// </summary>
        /// <param name="id">ID retka kojeg zelimo ispisati</param>
        /// <returns>novi pogled</returns>
        public PartialViewResult RowSastanak(int id)
        {
            var sastanak = ctx.Sastanak
                             .Where(m => m.SifraSastanka == id)
                             .Select(m => new SastanakViewModel
                             {
                                 SifraSastanka = m.SifraSastanka,
                                 Datum = m.Datum,
                                 NazivStozera = m.SifraStozeraNavigation.Naziv
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
        /// Brisanje zeljenog stozera
        /// </summary>
        /// <param name="id">ID stozera kojeg zelimo obrisati</param>
        /// <returns>JSON rezultat</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var stozer = ctx.Stozer
                             .AsNoTracking() //ima utjecaj samo za Update, za brisanje možemo staviti AsNoTracking
                             .Where(m => m.SifraStozera == id)
                             .SingleOrDefault();
            if (stozer != null)
            {
                try
                {
                    string naziv = stozer.Naziv;
                    ctx.Remove(stozer);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Stožer {naziv} sa šifrom {id} obrisan.",
                        successful = true
                    };
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = "Pogreška prilikom brisanja stožera: " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Stožer sa šifrom {id} ne postoji");
            }
        }

        /// <summary>
        /// Prikaz zeljene stranice tablice
        /// </summary>
        /// <param name="page">Redni broj stranice</param>
        /// <param name="sort">Redni broj stupca po kojem se sortira</param>
        /// <param name="ascending">Smjer sortiranje (true za uzlazno)</param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Stozer.Include(z => z.IdPredsjednikaNavigation).AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Stozer, object>> orderSelector = null;

            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Naziv;
                    break;
                case 2:
                    orderSelector = d => d.IdPredsjednikaNavigation.Ime + d.IdPredsjednikaNavigation.Prezime;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var stozeri = query
                      .Select(m => new StozerViewModel
                      {
                          ImePredsjednika = m.IdPredsjednikaNavigation.Ime + m.IdPredsjednikaNavigation.Prezime,
                          SifraStozera = m.SifraStozera,
                          Naziv = m.Naziv
                      })
                      .Skip((page - 1) * pagesize)
                      .Take(pagesize)
                      .ToList();


            var model = new StozeriViewModel
            {
                Stozeri = stozeri,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Stvaranje MD pogleda
        /// </summary>
        /// <param name="id"></param>
        /// <returns>MD pogled</returns>
        public async Task<IActionResult> Detail(int id)
        {
            if (id == null)
            {
                return NotFound();
            }


            List<SastanakViewModel> sastanci = new List<SastanakViewModel>();
            var sastanak = ctx.Sastanak
                             .Where(k => k.SifraStozera == id)
                             .Select(k => new SastanakViewModel
                             {
                                 NazivStozera = k.SifraStozeraNavigation.Naziv,
                                 Datum = k.Datum,
                                 SifraSastanka = k.SifraSastanka
                             })
                             .ToList();

            if (sastanak.Count != 0)
            {
                foreach (SastanakViewModel k in sastanak)
                {
                    sastanci.Add(k);
                }
            }

            var stozer = await ctx.Stozer
                            .Where(z => z.SifraStozera == id)
                            .Select(z => new StozerDetailsViewModel
                            {
                                SifraStozera = z.SifraStozera,
                                Naziv = z.Naziv,
                                ImePredsjednika = z.IdPredsjednikaNavigation.Prezime + z.IdPredsjednikaNavigation.Ime,
                                Sastanci = sastanci,
                            })
                            .SingleOrDefaultAsync();

            if (stozer == null)
            {
                return NotFound();
            }


            return View(stozer);
        }

        /// <summary>
        /// Brisanje sastanka U MD
        /// </summary>
        /// <param name="id">ID sastanka kojeg zelimo obrisati</param>
        /// <returns>JSON rezultat</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSastanak(int id)
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
        /// Izvoz tablice za Excel
        /// </summary>
        public void ExportToExcel()
        {
            List<StozerViewModel> emplist = ctx.Stozer.Select(x => new StozerViewModel
            {
                SifraStozera = x.SifraStozera,
                Naziv = x.Naziv,
                ImePredsjednika = x.IdPredsjednikaNavigation.Prezime.ToString().Trim() + " " + x.IdPredsjednikaNavigation.Ime.ToString().Trim()
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Stozeri");

            ws.Cells["A1"].Value = "Stozeri";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra Stozera";
            ws.Cells["B6"].Value = "Naziv stozera";
            ws.Cells["C6"].Value = "Ime predsjednika";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraStozera;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Naziv;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.ImePredsjednika;
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
        /// Izrada PDF izvjestaja
        /// </summary>
        /// <returns>izvjestaj u PDF formatu</returns>
        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis stozera";
            var stozeri = await ctx.Stozer
                .Include(o => o.IdPredsjednikaNavigation)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(stozeri));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Stozer>(o => o.SifraStozera);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra stozera", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Stozer>(o => o.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Naziv stozera", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Stozer>(o => o.IdPredsjednikaNavigation.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Ime predsjednika", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Stozer>(o => o.IdPredsjednikaNavigation.Prezime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Prezime predsjednika", horizontalAlignment: HorizontalAlignment.Left);
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