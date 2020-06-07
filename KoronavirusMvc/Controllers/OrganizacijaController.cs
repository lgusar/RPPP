using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using PdfRpt.FluentInterface;
using PdfRpt.Core.Contracts;

namespace KoronavirusMvc.Controllers
{
    /// <summary>
    /// Kontroler za organizaciju
    /// </summary>
    public class OrganizacijaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<OrganizacijaController> logger;
        /// <summary>
        /// Stvaranja kontrolera za organizaciju
        /// </summary>
        /// <param name="ctx">Postavljanje baze</param>
        /// <param name="optionsSnapshot">Postavljanje postavki stranice</param>
        /// <param name="logger">Postavljanje loggera</param>
        public OrganizacijaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<OrganizacijaController> logger)
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
            return View();
        }

        private async Task PrepareDropDownLists()
        {

            var organizacije = await ctx.Organizacija.OrderBy(d => d.SifraOrganizacije).Select(d => new { d.Naziv, d.SifraOrganizacije }).ToListAsync();
            ViewBag.Organizacije = new SelectList(organizacije, nameof(Organizacija.SifraOrganizacije), nameof(Organizacija.Naziv));

        }
        /// <summary>
        /// Kreiranje nove organizacije
        /// </summary>
        /// <param name="organizacija">Organizacija</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Organizacija organizacija)
        {
            logger.LogTrace(JsonSerializer.Serialize(organizacija), new JsonSerializerOptions { IgnoreNullValues = true });
            if (ModelState.IsValid)
            {
                try
                {
                    organizacija.SifraOrganizacije = (int)NewId();
                    ctx.Add(organizacija);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Organizacija {organizacija.SifraOrganizacije} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation(new EventId(1000), $"Organizacija {organizacija.Naziv} dodana");

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom dodavanja nove organizacije {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(organizacija);
                }
            }
            else
            {
                return View(organizacija);
            }
        }
        /// <summary>
        /// Ažuriranje organizacije s nekim id-om
        /// </summary>
        /// <param name="id">Sifra organizacije</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var organizacija = ctx.Organizacija
                             .AsNoTracking()
                             .Where(m => m.SifraOrganizacije == id)
                             .SingleOrDefault();
            if (organizacija != null)
            {
                return PartialView(organizacija);
            }
            else
            {
                return NotFound($"Neispravna sifra organizacije: {id}");
            }
        }

        /// <summary>
        /// Ažuriranje organizacije
        /// </summary>
        /// <param name="organizacija">Organizacija</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Organizacija organizacija)
        {
            logger.LogTrace(JsonSerializer.Serialize(organizacija), new JsonSerializerOptions { IgnoreNullValues = true });
            if (organizacija == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = ctx.Organizacija.Any(m => m.SifraOrganizacije == organizacija.SifraOrganizacije);
            if (!checkId)
            {
                return NotFound($"Neispravna sifra institucije: {organizacija?.SifraOrganizacije}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(organizacija);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Organizacija {organizacija.Naziv} ažurirana");
                    return StatusCode(302, Url.Action(nameof(Row), new { id = organizacija.SifraOrganizacije }));
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom ažuriranja organizacije {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return PartialView(organizacija);
                }
            }
            else
            {
                return PartialView(organizacija);
            }
        }

        /// <summary>
        /// Prikaz za atribute jedne organizacije s nekim id-om
        /// </summary>
        /// <param name="id">Sifra organizacije</param>
        /// <returns></returns>
        public PartialViewResult Row(int id)
        {
            var organizacija = ctx.Organizacija
                             .Where(m => m.SifraOrganizacije == id)
                             .Select(m => new Organizacija
                             {
                                 SifraOrganizacije = m.SifraOrganizacije,
                                 Naziv = m.Naziv,
                                 Url = m.Url,
                             })
                             .SingleOrDefault();
            if (organizacija != null)
            {
                return PartialView(organizacija);
            }
            else
            {
                return PartialView("ErrorMessageRow", $"Neispravan id institucije ovaj: {id}");
            }
        }


        /// <summary>
        /// Brisanje institucije s nekim id-om
        /// </summary>
        /// <param name="id">Sifra organizacije</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var organizacija = await ctx.Organizacija.FindAsync(id);
            logger.LogTrace(JsonSerializer.Serialize(organizacija), new JsonSerializerOptions { IgnoreNullValues = true });
            if (organizacija != null)
            {
                try
                {
                    string naziv = organizacija.Naziv;
                    ctx.Remove(organizacija);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation(new EventId(1000), $"Organizacija {organizacija.Naziv} obrisana");
                    var result = new
                    {
                        message = $"Organizacija {naziv} sa šifrom {id} obrisana",
                        successful = true
                    };

                    return Json(result);
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom brisanja organizacije {exc.CompleteExceptionMessage()}");
                    var result = new
                    {
                        message = $"Pogreška prilikom brisanja institucije {exc.CompleteExceptionMessage()}",
                        successful = false
                    };
                    return Json(result);
                }

            }
            else
            {
                return NotFound($"Organizacija sa šifrom {id} ne postoji");
            }
        }
        /// <summary>
        /// Prikaz tablice za Organizacije
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="sort">Sort</param>
        /// <param name="ascending">Ascending(true ili false)</param>
        /// <returns></returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Organizacija.AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Organizacija, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Naziv;
                    break;
                case 2:
                    orderSelector = d => d.Url;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var organizacije = query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToList();
            var model = new OrganizacijeViewModel
            {
                Organizacije = organizacije,
                PagingInfo = pagingInfo
            };
            return View(model);
        }
        /// <summary>
        /// Prikaz MD forme za organizaciju s nekim id-om
        /// </summary>
        /// <param name="id">Sifra organizacije</param>
        /// <param name="page">Page</param>
        /// <param name="sort">Sort</param>
        /// <param name="ascending">Ascending (true ili false)</param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Institucija.AsNoTracking();
            int count = query.Count();
            var query_2 = ctx.Preporuka.AsNoTracking();
            int count2 = query.Count();

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
            System.Linq.Expressions.Expression<Func<Institucija, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.SifraInstitucije;
                    break;
                case 2:
                    orderSelector = p => p.NazivInstitucije;
                    break;
                case 3:
                    orderSelector = p => p.Kontakt;
                    break;
                case 4:
                    orderSelector = p => p.RadnoVrijeme;
                    break;
            }
            if (orderSelector != null)
            {
                query = ascending ?
                    query.OrderBy(orderSelector) :
                    query.OrderByDescending(orderSelector);
            }

            var organizacija = ctx.Organizacija
                .AsNoTracking()
                .Where(m => m.SifraOrganizacije == id)
                .Select(o => new Organizacija
                {
                    SifraOrganizacije = o.SifraOrganizacije,
                    Naziv = o.Naziv,
                    Url = o.Url,
                })
                .SingleOrDefault(o => o.SifraOrganizacije == id);

            var preporuke = query_2
                .Select(p => new PreporukaViewModel
                {
                    SifraPreporuke = p.SifraPreporuke,
                    Opis = p.Opis,
                    NazivOrganizacije = p.SifraOrganizacijeNavigation.Naziv,
                    NazivStozera = p.SifraStozeraNavigation.Naziv,
                    OpisPrethodnePreporuke = p.SifraPrethodnePreporukeNavigation.Opis,
                    VrijemeObjave = p.VrijemeObjave
                })
                .Where(m => m.NazivOrganizacije == organizacija.Naziv)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            var model_2 = new PreporukeViewModel
            {
                Preporuke = preporuke,
                PagingInfo = pagingInfo
            };



            var institucije = query
                .Select(p => new InstitucijaViewModel
                {
                    SifraInstitucije = p.SifraInstitucije,
                    NazivInstitucije = p.NazivInstitucije,
                    Kontakt = p.Kontakt,
                    RadnoVrijeme = p.RadnoVrijeme,
                    NazivOrganizacije=p.SifraOrganizacijeNavigation.Naziv
                })
                .Where(m =>m.NazivOrganizacije == organizacija.Naziv)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();


            var model = new InstitucijeViewModel
            {
                Institucije = institucije,
                PagingInfo = pagingInfo
            };



            if (organizacija != null)
            {
                OrganizacijaInstitucijaPreporukaViewModel organizacijaInstitucija = new  OrganizacijaInstitucijaPreporukaViewModel
                {

                    Organizacija = organizacija,
                    Institucije = model,
                    Preporuke = model_2
                };
                return View(organizacijaInstitucija);
            }
            else
            {
                return NotFound($"Neispravan id institucije {id}");
            }
        }
        /// <summary>
        /// Ažuriranje institucije u MD formi s nekim id-om
        /// </summary>
        /// <param name="id">Sifra institucije</param>
        /// <returns></returns>
        public async Task<IActionResult> EditInstitucija(int id)
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
        /// Ažuriranje institucije u MD formi
        /// </summary>
        /// <param name="institucija">Institucija</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInstitucija(Institucija institucija)
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
                    return StatusCode(302, Url.Action(nameof(RowInstitucija), new { id = institucija.SifraInstitucije }));
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
        /// Brisanje institucije u MD formi s nekim id-em 
        /// </summary>
        /// <param name="id">Institucija</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteInstitucija(int id)
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
            else
            {
                return NotFound($"Institucija sa šifrom {id} ne postoji");
            }
        }
        /// <summary>
        /// Prikaz Institucije u MD formi s nekim id-em
        /// </summary>
        /// <param name="id">Sifra institucije</param>
        /// <returns></returns>
        public PartialViewResult RowInstitucija(int id)
        {
            var institucija = ctx.Institucija
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
        /// Brisanje preporuke u MD formi s nekim id-em
        /// </summary>
        /// <param name="id">Sifra preporuke</param>
        /// <returns></returns>
        public async Task<IActionResult> DeletePreporuka(int id)
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
        /// Ažuriranje preporuke u MD formi s nekim id-em
        /// </summary>
        /// <param name="id">Sifra preporuke</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditPreporuka(int id)
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
        /// Ažuriranje preporuke u MD formi
        /// </summary>
        /// <param name="preporuka">Preporuka</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPreporuka(Preporuka preporuka)
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
                    return StatusCode(302, Url.Action(nameof(RowPreporuka), new { id = preporuka.SifraPreporuke }));
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
        /// Prikaz atributa za preporuku u MD formi s nekim id-em
        /// </summary>
        /// <param name="id">Sifra preporuke</param>
        /// <returns></returns>
        public PartialViewResult RowPreporuka(int id)
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
        /// Povećava sifru organizacije
        /// </summary>
        /// <returns>Nova sifra organizacije</returns>
        private decimal NewId()
        {
            var maxId = ctx.Organizacija
                      .Select(o => o.SifraOrganizacije)
                      .ToList()
                      .Max();

            return maxId + 1;
        }
        /// <summary>
        /// Export u excel datoteku
        /// </summary>
        public void ExportToExcel()
        {
            List<Organizacija> emplist = ctx.Organizacija.Select(x => new Organizacija
            {
                SifraOrganizacije = x.SifraOrganizacije,
                Naziv = x.Naziv,
                Url = x.Url,
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Oprema");

            ws.Cells["A1"].Value = "Oprema";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A6"].Value = "Sifra Organizacije";
            ws.Cells["B6"].Value = "Naziv Organizacije";
            ws.Cells["C6"].Value = "URL organizacije";
    
            int rowStart = 7;
            foreach (var item in emplist)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SifraOrganizacije;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Naziv;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.Url;
                rowStart++;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Add("content-disposition", "attachment; filename=Organizacije.xlsx");
            Response.Body.WriteAsync(pck.GetAsByteArray());
            Response.CompleteAsync();

        }

        /// <summary>
        /// Export u pdf datoteku
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis organizacija";
            var organizacije = await ctx.Organizacija
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(organizacije));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Organizacija>(o => o.SifraOrganizacije);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Sifra organizacije", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Organizacija>(o => o.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(4);
                    column.HeaderCell("Naziv organizacije", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Organizacija>(o => o.Url);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("URL", horizontalAlignment: HorizontalAlignment.Center);
                });

            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=Organizacije.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }

        }
        /// <summary>
        /// Export MD forme u datoteku
        /// </summary>
        /// <param name="id">Sifra organizacije</param>
        public void ExportMDToExcel(int id)
        {

            var query = ctx.Institucija.AsNoTracking();
            int count = query.Count();
            var query_2 = ctx.Preporuka.AsNoTracking();
            int count2 = query.Count();



            var organizacija = ctx.Organizacija
                .AsNoTracking()
                .Where(m => m.SifraOrganizacije == id)
                .Select(o => new Organizacija
                {
                    SifraOrganizacije = o.SifraOrganizacije,
                    Naziv = o.Naziv,
                    Url = o.Url,
                })
                .SingleOrDefault(o => o.SifraOrganizacije == id);

            var preporuke = query_2
                .Select(p => new PreporukaViewModel
                {
                    SifraPreporuke = p.SifraPreporuke,
                    Opis = p.Opis,
                    NazivOrganizacije = p.SifraOrganizacijeNavigation.Naziv,
                    NazivStozera = p.SifraStozeraNavigation.Naziv,
                    OpisPrethodnePreporuke = p.SifraPrethodnePreporukeNavigation.Opis,
                    VrijemeObjave = p.VrijemeObjave
                })
                .Where(m => m.NazivOrganizacije == organizacija.Naziv);

            var model_2 = new PreporukeViewModel
            {
                Preporuke = preporuke,
            };



            var institucije = query
                .Select(p => new InstitucijaViewModel
                {
                    SifraInstitucije = p.SifraInstitucije,
                    NazivInstitucije = p.NazivInstitucije,
                    Kontakt = p.Kontakt,
                    RadnoVrijeme = p.RadnoVrijeme,
                    NazivOrganizacije = p.SifraOrganizacijeNavigation.Naziv
                })
                .Where(m => m.NazivOrganizacije == organizacija.Naziv);


            var model = new InstitucijeViewModel
            {
                Institucije = institucije,
            };

            OrganizacijaInstitucijaPreporukaViewModel organizacijaInstitucija = new OrganizacijaInstitucijaPreporukaViewModel
            {

                Organizacija = organizacija,
                Institucije = model,
                Preporuke = model_2
            };


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Institucija oprema");

            ws.Cells["A1"].Value = "OrganizacijaInstitucijaPreporukaMD";

            ws.Cells["A3"].Value = "Date";
            ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            ws.Cells["A5"].Value = "Organizacija";

            ws.Cells["A7"].Value = "Sifra Organizacije";
            ws.Cells["B7"].Value = "Naziv Organizacije";
            ws.Cells["C7"].Value = "URL";

            int rowStart = 8;

            ws.Cells[string.Format("A{0}", rowStart)].Value = organizacijaInstitucija.Organizacija.SifraOrganizacije;
            ws.Cells[string.Format("B{0}", rowStart)].Value = organizacijaInstitucija.Organizacija.Naziv;
            ws.Cells[string.Format("C{0}", rowStart)].Value = organizacijaInstitucija.Organizacija.Url;
       
            ws.Cells["A10"].Value = "Institucije ove organizacije";

            ws.Cells["A12"].Value = "Sifra Institucije";
            ws.Cells["B12"].Value = "Naziv institucije";
            ws.Cells["C12"].Value = "Radno vrijeme";
            ws.Cells["D12"].Value = "Kontakt";

            int rowStart2 = 13;

            foreach (var item in organizacijaInstitucija.Institucije.Institucije)
            {

                ws.Cells[string.Format("A{0}", rowStart2)].Value = item.SifraInstitucije;
                ws.Cells[string.Format("B{0}", rowStart2)].Value = item.NazivInstitucije;
                ws.Cells[string.Format("C{0}", rowStart2)].Value = item.RadnoVrijeme;
                ws.Cells[string.Format("D{0}", rowStart2)].Value = item.Kontakt;
                rowStart2++;
            }

            ws.Cells["A19"].Value = "Sifra Preporuke";
            ws.Cells["B19"].Value = "Opis Preporuke";
            ws.Cells["C19"].Value = "Organizacija";
            ws.Cells["D19"].Value = "Vrijeme objave";

            int rowStart3 = 20;

            foreach (var item in organizacijaInstitucija.Preporuke.Preporuke)
            {

                ws.Cells[string.Format("A{0}", rowStart3)].Value = item.SifraPreporuke;
                ws.Cells[string.Format("B{0}", rowStart3)].Value = item.Opis;
                ws.Cells[string.Format("C{0}", rowStart3)].Value = item.NazivOrganizacije;
                ws.Cells[string.Format("D{0}", rowStart3)].Value = item.VrijemeObjave.ToString();
                rowStart3++;
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