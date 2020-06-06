using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KoronavirusMvc.Extensions;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;

namespace KoronavirusMvc.Controllers
{
    public class KontaktController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<KontaktController> logger;
        public KontaktController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<KontaktController> logger)
        {
            this.ctx = ctx;
            appSettings = optionsSnapshot.Value;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Kontakt kontakt)
        {
            logger.LogTrace(JsonSerializer.Serialize(kontakt));
            if (ModelState.IsValid)
            {
                
                try
                { 
                    ctx.Add(kontakt);
                    ctx.SaveChanges();
                    TempData[Constants.Message] = $"Osoba {kontakt.IdOsoba} uspješno dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation($"Uspješno dodan kontakt");



                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom dodavanja kontakta {exc.CompleteExceptionMessage()}");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(kontakt);
                }
            }
            else
            {
                return View(kontakt);
            }
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Kontakt.Include(k => k.IdKontaktNavigation).Include(k => k.IdOsobaNavigation).AsNoTracking();

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

            System.Linq.Expressions.Expression<Func<Kontakt, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = k => k.IdOsoba;
                    break;
                case 2:
                    orderSelector = k => k.IdOsobaNavigation.Ime;
                    break;
                case 3:
                    orderSelector = k => k.IdOsobaNavigation.Prezime;
                    break;
                case 4:
                    orderSelector = k => k.IdKontaktNavigation.IdentifikacijskiBroj;
                    break;
                case 5:
                    orderSelector = k => k.IdKontaktNavigation.Ime;
                    break;
                case 6:
                    orderSelector = k => k.IdKontaktNavigation.Prezime;
                    break;


            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            var kontakti = query
                            .Select(z => new KontaktViewModel
                            {
                                IdOsobe = z.IdOsoba,
                                ImeOsoba = z.IdOsobaNavigation.Ime,
                                PrezimeOsoba = z.IdOsobaNavigation.Prezime,
                                IdKontakt = z.IdKontakt,
                                ImeKontakt = z.IdKontaktNavigation.Ime,
                                PrezimeKontakt = z.IdKontaktNavigation.Prezime
                            })
                            .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();
            var model = new KontaktiViewModel
            {
                Kontakti = kontakti,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string idOsoba, string idKontakt)
        {

            Kontakt kontakt = ctx.Kontakt.Where(k => (k.IdOsoba == idOsoba && k.IdKontakt == idKontakt) || (k.IdOsoba == idKontakt && k.IdKontakt == idOsoba)).FirstOrDefault();
            logger.LogTrace(JsonSerializer.Serialize(kontakt));
            if (kontakt == null )
            {
                return NotFound();
            }
            else
            {
                try
                {

                    ctx.Remove(kontakt);
                    ctx.SaveChanges();

                    logger.LogInformation($"Kontakt obrisan");
                    TempData[Constants.Message] = $"Osoba uspješno obrisana.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = $"Pogreška prilikom brisanja kontakta: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError($"Pogreška prilikom brisanja kontakta {exc.CompleteExceptionMessage()}");
                }
                return RedirectToAction("Index");
            }
        }


        public async Task<IActionResult> PDFReport()
        {
            string naslov = "Popis osoba u kontaktu";
            var kontkat = await ctx.Kontakt
                .Include(o => o.IdKontaktNavigation)
                .Include(o => o.IdOsobaNavigation)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(kontkat));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<Kontakt>(o => o.IdOsoba);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(4);
                    column.HeaderCell("Identifikacijski broj osobe", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Kontakt>(o => o.IdOsobaNavigation.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Kontakt>(o => o.IdOsobaNavigation.Prezime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Kontakt>(o => o.IdKontakt);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(4);
                    column.HeaderCell("Identifikacijski broj kontakta", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Kontakt>(o => o.IdKontaktNavigation.Ime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Left);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<Kontakt>(o => o.IdKontaktNavigation.Prezime);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Left);
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