using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Extensions;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using PdfRpt.FluentInterface;
using PdfRpt.Core.Contracts;

namespace KoronavirusMvc.Controllers
{
    /// <summary>
    /// Kontroler za institucije
    /// </summary>
    public class InstitucijaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<InstitucijaController> logger;
        /// <summary>
        /// Kreiranje kontrolera za institucije
        /// </summary>
        /// <param name="ctx">Postavljanje baze</param>
        /// <param name="optionsSnapshot">Postavljanje postavki stranice</param>
        /// <param name="logger">Postavljanje loggera</param>
        public InstitucijaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<InstitucijaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }
        /// <summary>
        /// Kod kreiranja izbaci dropdown odabir
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

            var organizacije = await ctx.Organizacija.OrderBy(d => d.SifraOrganizacije).Select(d => new { d.Naziv, d.SifraOrganizacije }).ToListAsync();
            ViewBag.Organizacije = new SelectList(organizacije, nameof(Organizacija.SifraOrganizacije), nameof(Organizacija.Naziv));

            var opreme = await ctx.Institucija.OrderBy(d => d.SifraInstitucije).Select(d => new { d.NazivInstitucije, d.SifraInstitucije }).ToListAsync();
            ViewBag.Institucije = new SelectList(opreme, nameof(Institucija.SifraInstitucije), nameof(Institucija.NazivInstitucije));
        }

        /// <summary>
        /// Stvaranje nove institucije
        /// </summary>
        /// <param name="institucija">Institucija</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Institucija institucija)
        {
            logger.LogTrace(JsonSerializer.Serialize(institucija), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    institucija.SifraInstitucije = (int)NewId();
                    ctx.Add(institucija);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Institucija {institucija.NazivInstitucije} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                   
                    logger.LogInformation(new EventId(1000), $"Organizacija {institucija.NazivInstitucije} dodana");
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom dodavanja nove institucije {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(institucija);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(institucija);
            }
        }

        /// <summary>
        /// Početna stranica za pregled institucija
        /// </summary>
        /// <param name="page">Stranica</param>
        /// <param name="sort">Sort</param>
        /// <param name="ascending">Ascending (true ili false)</param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Institucija.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Institucija, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.NazivInstitucije;
                    break;
                case 2:
                    orderSelector = d => d.RadnoVrijeme;
                    break;
                case 3:
                    orderSelector = d => d.Kontakt;
                    break;
                case 4:
                    orderSelector = d => d.SifraOrganizacijeNavigation.Naziv;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);

            }       
            
            var institucije = ctx.Institucija
                              .Select(m => new InstitucijaViewModel
                              {
                                  SifraInstitucije = m.SifraInstitucije,
                                  NazivInstitucije = m.NazivInstitucije,
                                  RadnoVrijeme = m.RadnoVrijeme,
                                  Kontakt = m.Kontakt,
                                  NazivOrganizacije = m.SifraOrganizacijeNavigation.Naziv
                              })
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();

            var model = new InstitucijeViewModel
            {
                Institucije = institucije,
                PagingInfo = pagingInfo
            };
            return View(model);

        }
        /// <summary>
        /// Dinamičko brisanje institucije sa određenom šifrom te institucije.
        /// </summary>
        /// <param name="id">Sifra institucije</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete(int id)
        {
            var institucija = await ctx.Institucija.FindAsync(id);
            logger.LogTrace(JsonSerializer.Serialize(institucija), new JsonSerializerOptions { IgnoreNullValues = true });
            if (institucija != null)
            {
                try
                {
                    string naziv = institucija.NazivInstitucije;
                    ctx.Remove(institucija);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation(new EventId(1000), $"Organizacija {institucija.NazivInstitucije} obrisana");
                    var result = new
                    {
                        message = $"Institucija {naziv} sa šifrom {id} obrisana",
                        successful = true
                    };

                    return Json(result);
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom brisanja institucije {exc.CompleteExceptionMessage()}");
                    var result = new
                    {
                        message = $"Pogreška prilikom brisanja institucije {exc.CompleteExceptionMessage()}",
                        successful = false
                    };
                    return Json(result);
                }

            }
            else {
                return NotFound($"Institucija sa šifrom {id} ne postoji");
            }
        }
        /// <summary>
        /// Dinamičko ažuriranje institucije sa određenom šifrom
        /// </summary>
        /// <param name="id">Sifra institucije</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var institucija = ctx.Institucija
                             .AsNoTracking()
                             .Where(m => m.SifraInstitucije == id)
                             .SingleOrDefault();
            if (institucija != null)
            {
                await PrepareDropDownLists();
                return PartialView(institucija);
            }
            else
            {
                return NotFound($"Neispravna sifra institucije: {id}");
            }
        }
        /// <summary>
        /// Dinamičko ažuriranje odabrane institucije
        /// </summary>
        /// <param name="institucija">Institucija</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Institucija institucija)
        {
            logger.LogTrace(JsonSerializer.Serialize(institucija), new JsonSerializerOptions { IgnoreNullValues = true });
            if (institucija == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Institucija.Any(m => m.SifraInstitucije == institucija.SifraInstitucije);
            if (!checkId)
            {
                return NotFound($"Neispravna sifra institucije: {institucija?.SifraInstitucije}");
            }

            await PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(institucija);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Organizacija {institucija.NazivInstitucije} ažurirana");
                    return StatusCode(302, Url.Action(nameof(Row), new { id = institucija.SifraInstitucije }));
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom ažuriranja institucije {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(institucija);
                }
            }
            else
            {
                return PartialView(institucija);
            }
    }
        /// <summary>
        /// Funkcija za ispis jedne od institucija sa zadanom šifrom
        /// </summary>
        /// <param name="id">Sifra institucije</param>
        /// <returns></returns>
        public PartialViewResult Row(int id)
        {
            var institucija= ctx.Institucija
                             .Where(m => m.SifraInstitucije == id)
                             .Select(m => new InstitucijaViewModel
                             {
                                 SifraInstitucije = m.SifraInstitucije,
                                 NazivInstitucije = m.NazivInstitucije,
                                 RadnoVrijeme = m.RadnoVrijeme,
                                 Kontakt = m.Kontakt,
                                 NazivOrganizacije = m.SifraOrganizacijeNavigation.Naziv
                             })
                             .SingleOrDefault();
            if (institucija != null)
            {
                return PartialView(institucija);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan id institucije ovaj: {id}");
            }
        }

        /// <summary>
        /// Pregled detaila (opreme) neke institucije, te više informacija o samoj instituciji
        /// </summary>
        /// <param name="id">Sifra institucije</param>
        /// <param name="page">Page</param>
        /// <param name="sort">Sort</param>
        /// <param name="ascending">Ascending (true ili false)</param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id, int page = 1, int sort = 1, bool ascending = true)
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
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages, sort = sort, ascending = ascending });
            }
            System.Linq.Expressions.Expression<Func<Oprema, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.SifraOpreme;
                    break;
                case 2:
                    orderSelector = p => p.SifraInstitucijeNavigation.NazivInstitucije;
                    break;
                case 3:
                    orderSelector = p => p.NazivOpreme;
                    break;
                case 4:
                    orderSelector = p => p.KolicinaOpreme;
                    break;
            }
            if (orderSelector != null)
            {
                query = ascending ?
                    query.OrderBy(orderSelector) :
                    query.OrderByDescending(orderSelector);
            }

            var institucija = ctx.Institucija
                .AsNoTracking()
                .Where(m => m.SifraInstitucije == id)
                .Select(o => new InstitucijaViewModel
                {
                    SifraInstitucije = o.SifraInstitucije,
                    NazivInstitucije = o.NazivInstitucije,
                    RadnoVrijeme = o.RadnoVrijeme,
                    Kontakt = o.Kontakt,
                    NazivOrganizacije = o.SifraOrganizacijeNavigation.Naziv
                })
                .SingleOrDefault(o => o.SifraInstitucije == id);



            var opreme = query
                .Select(p => new OpremaViewModel
                {
                    SifraOpreme = p.SifraOpreme,
                    NazivInstitucije = p.SifraInstitucijeNavigation.NazivInstitucije,
                    NazivOpreme = p.NazivOpreme,
                    KolicinaOpreme = p.KolicinaOpreme,
                })
                .Where(m=>m.NazivInstitucije==institucija.NazivInstitucije)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();


            var model = new OpremeViewModel
            {
                Opremas = opreme,
                PagingInfo = pagingInfo
            };



            if (institucija != null)
            {
                InstitucijaOpremaViewModel osobaPregledi = new InstitucijaOpremaViewModel
                {
                    Institucija = institucija,
                    Oprema = model
                };
                await PrepareDropDownLists();
                return View(osobaPregledi);
            }
            else
            {
                return NotFound($"Neispravan id institucije {id}");
            }
        }

        /// <summary>
        /// Dinamičko brisanje detaila (opreme) pregledavane institucije
        /// </summary>
        /// <param name="id">Sifra opreme</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteOprema(int id)
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
        /// Dinamičko ažuriranje detaila(opreme) pregledavane institucije
        /// </summary>
        /// <param name="id">Sifra opreme</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditOprema(int id)
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
        /// Dinamičko ažuriranje detaila(opreme) pregledavane institucije
        /// </summary>
        /// <param name="oprema">Sifra opreme</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOprema(Oprema oprema)
        {
            if (oprema == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Oprema.Any(m => m.SifraOpreme == oprema.SifraOpreme);
            if (!checkId)
            {
                return NotFound($"Neispravna sifra opreme: {oprema?.SifraOpreme}");
            }

            await PrepareDropDownLists();
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(oprema);
                    ctx.SaveChanges();
                    return StatusCode(302, Url.Action(nameof(RowOprema), new { id = oprema.SifraOpreme }));
                }
                catch (Exception exc)
                {
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
        /// Funkcija za ispis opreme sa zadanom šifrom iz pregledavane institucije.
        /// </summary>
        /// <param name="id">Sifra opreme</param>
        /// <returns></returns>
        public PartialViewResult RowOprema(int id)
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
        /// Funkcija za određivanje id-a novo stvorene institucije
        /// </summary>
        /// <returns>Id nove institucije</returns>
        private decimal NewId()
        {
            var maxId = ctx.Institucija
                      .Select(o => o.SifraInstitucije)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
        /// <summary>
        /// Export tablice u Excel
        /// </summary>
        public void ExportToExcel()
        {
            List<InstitucijaViewModel> emplist = ctx.Institucija.Select(o => new InstitucijaViewModel
            {
                SifraInstitucije = o.SifraInstitucije,
                NazivInstitucije = o.NazivInstitucije,
                RadnoVrijeme = o.RadnoVrijeme,
                Kontakt = o.Kontakt,
                NazivOrganizacije = o.SifraOrganizacijeNavigation.Naziv
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Oprema");

            ws.Cells["A1"].Value = "Oprema";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra Institucije";
            ws.Cells["B6"].Value = "Naziv Institucije";
            ws.Cells["C6"].Value = "Radno vrijeme";
            ws.Cells["D6"].Value = "Kontakt";
            ws.Cells["E6"].Value = "Organizacija";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraInstitucije;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.NazivInstitucije;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.RadnoVrijeme;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.Kontakt;
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.NazivOrganizacije;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=Institucije.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }
        /// <summary>
        /// Export tablice u PDF datoteku
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis institucija";
            var institucije = await ctx.Institucija
                .Include(o=>o.SifraOrganizacijeNavigation)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(institucije));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Institucija>(o => o.SifraInstitucije);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra Institucije", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Institucija>(o => o.NazivInstitucije);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Naziv institucije", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Institucija>(o => o.RadnoVrijeme);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Radno vrijeme", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Institucija>(o => o.Kontakt);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Kontakt", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Institucija>(o => o.SifraOrganizacijeNavigation.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Organizacija", horizontalAlignment: HorizontalAlignment.Left);
                });


            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Institucije.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }
        /// <summary>
        /// Export master detail tablice u excel datotetku (xls)
        /// </summary>
        /// <param name="id"></param>
        public void ExportMDToExcel(int id)
        {
            var query = ctx.Oprema.AsNoTracking();

            var institucija = ctx.Institucija
                .AsNoTracking()
                .Where(m => m.SifraInstitucije == id)
                .Select(o => new InstitucijaViewModel
                {
                    SifraInstitucije = o.SifraInstitucije,
                    NazivInstitucije = o.NazivInstitucije,
                    RadnoVrijeme = o.RadnoVrijeme,
                    Kontakt = o.Kontakt,
                    NazivOrganizacije = o.SifraOrganizacijeNavigation.Naziv
                })
                .SingleOrDefault(o => o.SifraInstitucije == id);



            var opreme = query
                .Select(p => new OpremaViewModel
                {
                    SifraOpreme = p.SifraOpreme,
                    NazivInstitucije = p.SifraInstitucijeNavigation.NazivInstitucije,
                    NazivOpreme = p.NazivOpreme,
                    KolicinaOpreme = p.KolicinaOpreme,
                })
                .Where(m => m.NazivInstitucije == institucija.NazivInstitucije);


            var model = new OpremeViewModel
            {
                Opremas = opreme,
            };

            InstitucijaOpremaViewModel osobaPregledi = new InstitucijaOpremaViewModel
            {
                Institucija = institucija,
                Oprema = model
            };

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Institucija oprema");

            ws.Cells["A1"].Value = "InstitucijaOpremaMD";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A5"].Value = "Institucija";

            ws.Cells["A7"].Value = "Sifra Institucije";
            ws.Cells["B7"].Value = "Naziv Institucije";
            ws.Cells["C7"].Value = "Radno vrijeme";
            ws.Cells["D7"].Value = "Kontakt";

            int rowStart = 8;

            ws.Cells[string.Format("A{0}", rowStart)].Value = osobaPregledi.Institucija.SifraInstitucije;
            ws.Cells[string.Format("B{0}", rowStart)].Value = osobaPregledi.Institucija.NazivInstitucije;
            ws.Cells[string.Format("C{0}", rowStart)].Value = osobaPregledi.Institucija.RadnoVrijeme;
            ws.Cells[string.Format("D{0}", rowStart)].Value = osobaPregledi.Institucija.Kontakt;

            ws.Cells["A10"].Value = "Oprema u instituciji";

            ws.Cells["A12"].Value = "Sifra Opreme";
            ws.Cells["B12"].Value = "Naziv institucije";
            ws.Cells["C12"].Value = "Naziv opreme";
            ws.Cells["D12"].Value = "Kolicina opreme";

            int rowStart2 = 13;

            foreach (var item in osobaPregledi.Oprema.Opremas)
            {

                ws.Cells[string.Format("A{0}", rowStart2)].Value = item.SifraOpreme;
                ws.Cells[string.Format("B{0}", rowStart2)].Value = item.NazivInstitucije;
                ws.Cells[string.Format("C{0}", rowStart2)].Value = item.NazivOpreme;
                ws.Cells[string.Format("D{0}", rowStart2)].Value = item.KolicinaOpreme;
                rowStart2++;
            }


            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=InstitucijaMD.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }

    }
}