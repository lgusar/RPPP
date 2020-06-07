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
    /// <summary>
    /// Razred za backend rad s osobama i tablicama vezanim uz tablicu osoba
    /// </summary>
    public class OsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<OsobaController> logger;
        /// <summary>
        /// Kontruktor razreda OsobaContorller
        /// </summary>
        /// <param name="ctx">Kontekst baze</param>
        /// <param name="optionsSnapshot">Opcije app</param>
        /// <param name="logger">Logger za ispis logova prilikom dodavanja, brisanja i ažuriranja u bazi podataka</param>
        public OsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<OsobaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = optionsSnapshot.Value;
        }

        /// <summary>
        /// Metoda koja služi za dohvaćanje Create.cshtml stranice za stvaranje nove osobe
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Metoda koja stvara novu osobu u bazi podataka
        /// </summary>
        /// <param name="osoba">Model osobe sa svim atributima iz tablice osoba</param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja služi za dohvaćanje Edit.cshtml stranice za ažuriranje osobe
        /// </summary>
        /// <param name="id">Identifikacijski broj osobe koju želimo ažurirati</param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja ažurira podatke osobe
        /// </summary>
        /// <param name="id">Identidikacijski broj osobe koju ažuriramo</param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja služi za brisanje osobe iz baze podataka
        /// </summary>
        /// <param name="id">IIdentifikacijski broj osobe koju brišemo iz baze podataka</param>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja služi za tablični prikaz osoba u bazi podataka
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja vraća prikaz detalja neke osobe sa svim ostalim podacima kojih nema u tablici Osoba
        /// </summary>
        /// <param name="id">Identifikacijski broj osobe</param>
        /// <returns></returns>
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


        /// <summary>
        /// Metoda koja dinamički briše osobu koja je označena kao zaražena
        /// </summary>
        /// <param name="id">Identifikacijski broj osobe</param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja priprema padajuću listu za stanja koja su u bazi podataka
        /// </summary>
        private void PrepareDropDownLists()
        {
            var stanja = ctx.Stanje.OrderBy(s => s.NazivStanja).Select(s => new { s.NazivStanja, s.SifraStanja }).ToList();
            ViewBag.Stanja = new SelectList(stanja, nameof(Stanje.SifraStanja), nameof(Stanje.NazivStanja));
        }

        /// <summary>
        /// Metoda koja generira izvješće u pdf formatu. Stvara tablični prikaz svih osoba u bazi podataka
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja služi za dohvaćanje EditZarazenaOsoba.cshtml za ažuriranje zaražene osobe
        /// </summary>
        /// <param name="id">Identifikacijski broj osobe</param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja dinamički ažurira podatke zaražene osobe u detaljima osobe
        /// </summary>
        /// <param name="zarazenaOsoba">Model zaražene osobe koja se onda ažurira</param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja vraća parcijalni pogled za zaraženu osobu
        /// </summary>
        /// <param name="id">Identifikacijski broj osobe</param>
        /// <returns></returns>
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

        /// <summary>
        /// Metoda koja generira izvješće u Excelu. Vraća tablični popis osoba u bazi podataka
        /// </summary>
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

        /// <summary>
        /// Metoda koja generira izvješće u Excelu. Stvara tablicu sa svim podacima u detaljima osobe.
        /// </summary>
        public void ExportToExcelOsoba(string id)
        {
            var osoba = ctx.Osoba.Where(o => o.IdentifikacijskiBroj == id).FirstOrDefault();

            var zarazenaOsoba = ctx.ZarazenaOsoba.Include(o => o.SifraStanjaNavigation).AsNoTracking().Where(o => o.IdentifikacijskiBroj == id).FirstOrDefault();

            bool zarazena = false;

            if(zarazenaOsoba != null)
            {
                zarazena = true;
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

            ws.Cells["A7"].Value = osoba.IdentifikacijskiBroj;
            ws.Cells["B7"].Value = osoba.Ime;
            ws.Cells["C7"].Value = osoba.Prezime;
            ws.Cells["D7"].Value = osoba.Adresa;
            ws.Cells["E7"].Value = osoba.DatRod;
            ws.Cells["F7"].Value = osoba.Zanimanje;
            ws.Cells["G7"].Value = zarazena.Equals(true) ? "Da" : "Ne";
            ws.Cells["H7"].Value = kontakti.Count();

            int rowStart = 11;
            if (zarazenaOsoba != null)
            {
                ws.Cells["I6"].Value = "Datum zaraze";
                ws.Cells["J6"].Value = "Stanje osobe";

                ws.Cells["I7"].Value = string.Format("{0:dd.MM.yyyy}", zarazenaOsoba.DatZaraze);
                ws.Cells["J7"].Value = zarazenaOsoba.SifraStanjaNavigation.NazivStanja;

                ws.Cells["A10"].Value = "Kontakti";

                rowStart = 11;
                if (kontakti.Count != 0)
                {
                    foreach (KontaktViewModel s in kontakti)
                    {
                        ws.Cells[string.Format("A{0}", rowStart)].Value = s.IdKontakt;
                        ws.Cells[string.Format("B{0}", rowStart)].Value = s.ImeKontakt;
                        ws.Cells[string.Format("C{0}", rowStart)].Value = s.PrezimeKontakt;
                        rowStart++;
                    }
                }
            }
            else
            {
                ws.Cells["A10"].Value = "Kontakti";
                rowStart = 11;
                if (kontakti.Count != 0)
                {
                    foreach (KontaktViewModel s in kontakti)
                    {
                        ws.Cells[string.Format("A{0}", rowStart)].Value = s.IdKontakt;
                        ws.Cells[string.Format("B{0}", rowStart)].Value = s.ImeKontakt;
                        ws.Cells[string.Format("C{0}", rowStart)].Value = s.PrezimeKontakt;
                        rowStart++;
                    }
                }
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", $"attachment; filename=osoba{osoba.IdentifikacijskiBroj}.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }

        /// <summary>
        /// Metoda koja služi za dinamičko brisanje kontakta.
        /// </summary>
        /// <param name="idOsobe">Identifikacijski broj osobe</param>
        /// <param name="idKontakt">Identifikacijski broj kontakta</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteKontakt(string idOsobe, string idKontakt)
        {
            var kontakt = ctx.Kontakt
                             .AsNoTracking()
                             .Where(m => (m.IdOsoba == idOsobe && m.IdKontakt == idKontakt) || (m.IdKontakt == idOsobe && m.IdOsoba == idKontakt))
                             .SingleOrDefault();
            logger.LogTrace(JsonSerializer.Serialize(kontakt));
            if (kontakt != null)
            {
                try
                {

                    ctx.Remove(kontakt);
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
                        message = "Pogreška prilikom brisanja kontakta : " + exc.CompleteExceptionMessage(),
                        successful = false
                    };
                    logger.LogError($"Pogreška prilikom brisanja kontakta {exc.CompleteExceptionMessage()}");
                    return Json(result);
                }
            }
            else
            {
                return NotFound($"Kontakt s identifikacijskim brojem {idKontakt} ne postoji");
            }
        }

        /// <summary>
        /// Metoda koja generira pdf izvješće za detalje neke osobe.
        /// </summary>
        /// <param name="id">Identifikacijski broj osobe</param>
        /// <returns></returns>
        public async Task<IActionResult> PDFReportOsoba(string id)
        {
            string naslov = "Detalji osobe";
            var osobe = await ctx.Osoba
                .Include(o => o.KontaktIdKontaktNavigation)
                .Include(o => o.ZarazenaOsoba)
                .Where(o => o.IdentifikacijskiBroj == id)
                .Select(o => new OsobaDetailsViewModel
                {
                    IdentifikacijskiBroj = o.IdentifikacijskiBroj,
                    Ime = o.Ime,
                    Prezime = o.Prezime,
                    Adresa = o.Adresa,
                    DatRod = o.DatRod,
                    Zanimanje = o.Zanimanje,
                    Zarazena = o.ZarazenaOsoba.IdentifikacijskiBroj.Equals(id) ? true : false,
                    Zarazenastring = o.ZarazenaOsoba.IdentifikacijskiBroj.Equals(id) ? "Da" : "Ne",
                    DatZaraze = o.ZarazenaOsoba.DatZaraze,
                    NazivStanja = o.ZarazenaOsoba.SifraStanjaNavigation.NazivStanja
                })
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
                    column.PropertyName<OsobaDetailsViewModel>(o => o.IdentifikacijskiBroj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Identifikacijski broj", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<OsobaDetailsViewModel>(o => o.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<OsobaDetailsViewModel>(o => o.Prezime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<OsobaDetailsViewModel>(o => o.Adresa);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Adresa", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<OsobaDetailsViewModel>(o => o.DatRod);
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
                    column.PropertyName<OsobaDetailsViewModel>(o => o.Zanimanje);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Zanimanje", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<OsobaDetailsViewModel>(o => o.Zarazenastring);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Zaražena?", horizontalAlignment: HorizontalAlignment.Left);
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