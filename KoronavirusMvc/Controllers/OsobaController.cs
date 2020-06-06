using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;


namespace KoronavirusMvc.Controllers
{
    public class OsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<OsobaController> logger;
        public OsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<OsobaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Osoba osoba)
        {
            logger.LogTrace(JsonSerializer.Serialize(osoba));
            if (ModelState.IsValid)
            {

                try
                {
                    ctx.Add(osoba);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Osoba {osoba.Ime} {osoba.Prezime} uspješno dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation($"Osoba {osoba.Ime} {osoba.Prezime} dodana");

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom dodavanja osobe {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(osoba);
                }
            }
            else
            {
                return View(osoba);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id, int page = 1, int sort = 1, bool ascending = true)
        {
            var osoba = ctx.Osoba.AsNoTracking().Where(o => o.IdentifikacijskiBroj == id).FirstOrDefault();
            if (osoba == null)
            {
                return NotFound($"Ne postoji osoba s identifikacijskim brojem {id}");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(osoba);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Osoba osoba = await ctx.Osoba.FindAsync(id);
                if (osoba == null)
                {
                    return NotFound($"Ne postoji osoba s identifikacijskim brojem {id}");
                }
                logger.LogTrace(JsonSerializer.Serialize(osoba));
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                bool ok = await TryUpdateModelAsync<Osoba>(osoba, "", o => o.Ime, o => o.Prezime, o => o.Adresa, o => o.DatRod, o => o.Zanimanje);
                if (ok)
                {
                    try
                    {
                        string punoime = osoba.Ime + " " + osoba.Prezime;
                        TempData[Constants.Message] = $"Podaci osobe {punoime} uspješno ažurirani.";
                        TempData[Constants.ErrorOccurred] = false;
                        await ctx.SaveChangesAsync();
                        logger.LogInformation($"Osoba {osoba.Ime} {osoba.Prezime} ažurirana");
                        return RedirectToAction(nameof(Index), new { page, sort, ascending });
                    }
                    catch (Exception exc)
                    {
                        logger.LogError($"Pogreška prilikom ažuriranja podataka osobe {exc.CompleteExceptionMessage()}");
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(osoba);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o osobi nije moguće povezati s forme");
                    return View(osoba);
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
        public IActionResult Delete(string id, int page = 1, int sort = 1, bool ascending = true)
        {
            var osoba = ctx.Osoba.AsNoTracking().Where(o => o.IdentifikacijskiBroj == id).SingleOrDefault();
            logger.LogTrace(JsonSerializer.Serialize(osoba));
            if (osoba == null)
            {
                return NotFound($"Osoba s identifikacijski brojem {id} ne postoji.");
            }
            else
            {
                try
                {
                    string punoime = osoba.Ime + " " + osoba.Prezime;
                    ctx.Remove(osoba);
                    ctx.SaveChanges();
                    var result = new
                    {
                        message = $"Osoba {punoime} obrisana.",
                        successful = true
                    };
                    logger.LogInformation($"Osoba {osoba.Ime} {osoba.Prezime} obrisana");
                    return Json(result);
                }
                catch (Exception exc)
                {
                    var result = new
                    {
                        message = $"Pogreška prilikom brisanja osobe. {exc.CompleteExceptionMessage()}",
                        successful = false
                    };
                    logger.LogError($"Pogreška prilikom brisanja osobe {exc.CompleteExceptionMessage()}");
                    return Json(result);
                }
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Osoba.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Osoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = o => o.IdentifikacijskiBroj;
                    break;
                case 2:
                    orderSelector = o => o.Ime;
                    break;
                case 3:
                    orderSelector = o => o.Prezime;
                    break;
                case 4:
                    orderSelector = o => o.Adresa;
                    break;
                case 5:
                    orderSelector = o => o.DatRod;
                    break;
                case 6:
                    orderSelector = o => o.Zanimanje;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var osobe = query
                            .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new OsobeViewModel
            {
                Osobe = osobe,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }


            List<KontaktViewModel> kontakti = new List<KontaktViewModel>();
            var kontakt = ctx.Kontakt
                             .Where(k => k.IdOsoba == id)
                             .Select(k => new KontaktViewModel
                             {
                                 IdOsobe = k.IdOsoba,
                                 IdKontakt = k.IdKontakt,
                                 ImeOsoba = k.IdOsobaNavigation.Ime,
                                 PrezimeOsoba = k.IdOsobaNavigation.Prezime,
                                 ImeKontakt = k.IdKontaktNavigation.Ime,
                                 PrezimeKontakt = k.IdKontaktNavigation.Prezime
                             })
                             .ToList();
            var kontakt2 = ctx.Kontakt
                              .Where(k => k.IdKontakt == id)
                              .Select(k => new KontaktViewModel
                              {
                                  IdOsobe = k.IdKontakt,
                                  IdKontakt = k.IdOsoba,
                                  ImeOsoba = k.IdKontaktNavigation.Ime,
                                  PrezimeOsoba = k.IdKontaktNavigation.Prezime,
                                  ImeKontakt = k.IdOsobaNavigation.Ime,
                                  PrezimeKontakt = k.IdOsobaNavigation.Prezime
                              })
                              .ToList();
            if (kontakt.Count != 0)
            {
                foreach (KontaktViewModel k in kontakt)
                {
                    kontakti.Add(k);
                }
            }
            if (kontakt2.Count != 0)
            {
                foreach (KontaktViewModel k in kontakt2)
                {
                    kontakti.Add(k);
                }
            }

            var zarazena = ctx.ZarazenaOsoba
                              .Where(z => z.IdentifikacijskiBroj == id)
                              .FirstOrDefault();
            var osoba = await ctx.Osoba
                            .Where(z => z.IdentifikacijskiBroj == id)
                            .Select(z => new OsobaDetailsViewModel
                            {
                                IdentifikacijskiBroj = z.IdentifikacijskiBroj,
                                Ime = z.Ime,
                                Prezime = z.Prezime,
                                Adresa = z.Adresa,
                                DatRod = z.DatRod,
                                Zanimanje = z.Zanimanje,
                                DatZaraze = z.ZarazenaOsoba.DatZaraze,
                                Zarazena = z.ZarazenaOsoba.IdentifikacijskiBroj.Equals(id) ? true : false,
                                Zarazenastring = z.ZarazenaOsoba.IdentifikacijskiBroj.Equals(id) ? "Da" : "Ne",
                                NazivStanja = z.ZarazenaOsoba.SifraStanjaNavigation.NazivStanja,
                                Kontakti = kontakti,
                                ZarazenaOsoba = z.ZarazenaOsoba
                            })
                            .SingleOrDefaultAsync();

            //var osoba = await ctx.Osoba
            //    .FirstOrDefaultAsync(m => m.IdentifikacijskiBroj == id);
            if (osoba == null)
            {
                return NotFound();
            }


            return View(osoba);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteZarazenaOsoba(string id)
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


        private void PrepareDropDownLists()
        {
            var stanja = ctx.Stanje.OrderBy(s => s.NazivStanja).Select(s => new { s.NazivStanja, s.SifraStanja }).ToList();
            ViewBag.Stanja = new SelectList(stanja, nameof(Stanje.SifraStanja), nameof(Stanje.NazivStanja));
        }

        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis osoba";
            var osobe = await ctx.Osoba
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(osobe));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.IdentifikacijskiBroj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Identifikacijski broj", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.Prezime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.Adresa);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Adresa", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.DatRod);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Datum rođenja", horizontalAlignment: HorizontalAlignment.Center);
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
                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.Zanimanje);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Zanimanje", horizontalAlignment: HorizontalAlignment.Left);
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

        [HttpGet]
        public IActionResult EditZarazenaOsoba(string id)
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
        public IActionResult EditZarazenaOsoba(ZarazenaOsoba zarazenaOsoba)
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
                                        DatZaraze = z.DatZaraze,
                                        NazivStanja = z.SifraStanjaNavigation.NazivStanja
                                    })
                                    .SingleOrDefault();
            if (zarazenaOsoba != null)
            {
                return PartialView(zarazenaOsoba);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan identifikacijski broj osobe.");
            }
        }

        public void ExportToExcel()
        {
            List<Osoba> emplist = ctx.Osoba.Select(x => new Osoba
            {
                IdentifikacijskiBroj = x.IdentifikacijskiBroj,
                Ime = x.Ime,
                Prezime = x.Prezime,
                Adresa = x.Adresa,
                DatRod = x.DatRod,
                Zanimanje = x.Zanimanje
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Osobe");

            ws.Cells["A1"].Value = "Osobe";

            ws.Cells["A3"].Value = "Datum";
            ws.Cells["B3"].Value = string.Format("{0:dd.MM.yyyy} u {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Identifikacijski broj osobe";
            ws.Cells["B6"].Value = "Ime";
            ws.Cells["C6"].Value = "Prezime";
            ws.Cells["D6"].Value = "Adresa";
            ws.Cells["E6"].Value = "Datum rođenja";
            ws.Cells["F6"].Value = "Zanimanje";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.IdentifikacijskiBroj;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Ime;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.Prezime;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.Adresa;
                ws.Cells[string.Format("E{0}", rowStart)].Value = string.Format("{0:dd.MM.yyyy}", item.DatRod);
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.Zanimanje;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=myfile.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }

        public void ExportToExcelOsoba()
        {
            List<OsobaDetailsViewModel> emplist = ctx.Osoba.Include(o => o.ZarazenaOsoba).Select(x => new OsobaDetailsViewModel
            {
                IdentifikacijskiBroj = x.IdentifikacijskiBroj,
                Ime = x.Ime,
                Prezime = x.Prezime,
                Adresa = x.Adresa,
                DatRod = x.DatRod,
                Zanimanje = x.Zanimanje,
                Zarazenastring = x.ZarazenaOsoba.IdentifikacijskiBroj.Equals(x.IdentifikacijskiBroj) == true ? "Da" : "Ne",
                BrojKontakta = x.KontaktIdKontaktNavigation.Count() + x.KontaktIdOsobaNavigation.Count()
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Osoba");

            ws.Cells["A1"].Value = "Osoba";

            ws.Cells["A3"].Value = "Datum";
            ws.Cells["B3"].Value = string.Format("{0:dd.MM.yyyy} u {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Identifikacijski broj osobe";
            ws.Cells["B6"].Value = "Ime";
            ws.Cells["C6"].Value = "Prezime";
            ws.Cells["D6"].Value = "Adresa";
            ws.Cells["E6"].Value = "Datum rođenja";
            ws.Cells["F6"].Value = "Zanimanje";
            ws.Cells["G6"].Value = "Zaražena?";
            ws.Cells["H6"].Value = "Broj osoba u kontaktu";

            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.IdentifikacijskiBroj;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Ime;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.Prezime;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.Adresa;
                ws.Cells[string.Format("E{0}", rowStart)].Value = string.Format("{0:dd.MM.yyyy}", item.DatRod);
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.Zanimanje;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.Zarazenastring;
                ws.Cells[string.Format("H{0}", rowStart)].Value = item.BrojKontakta;
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