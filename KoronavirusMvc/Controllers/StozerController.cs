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
    public class StozerController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly AppSettings appSettings;
        private readonly ILogger<StozerController> logger;

        public StozerController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> optionsSnapshot, ILogger<StozerController> logger)
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

        /// <summary>
        /// 
        /// </summary>
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


        //[HttpGet]
        //public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    var stozer = ctx.Stozer.AsNoTracking().Where(d => d.SifraStozera == id).SingleOrDefault();
        //    if (stozer == null)
        //    {
        //        return NotFound("Ne postoji stožer s oznakom: " + id);
        //    }
        //    else
        //    {
        //        ViewBag.Page = page;
        //        ViewBag.Sort = sort;
        //        ViewBag.Ascending = ascending;
        //        PrepareDropDownLists();
        //        return View(stozer);
        //    }
        //}


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


        //[HttpPost, ActionName("Edit")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    try
        //    {
        //        Stozer stozer = await ctx.Stozer
        //                          .Where(d => d.SifraStozera == id)
        //                          .FirstOrDefaultAsync();
        //        if (stozer == null)
        //        {
        //            return NotFound("Neispravna šifra stožera: " + id);
        //        }

        //        if (await TryUpdateModelAsync<Stozer>(stozer, "",
        //            d => d.Naziv, d => d.IdPredsjednika
        //        ))
        //        {
        //            ViewBag.Page = page;
        //            ViewBag.Sort = sort;
        //            ViewBag.Ascending = ascending;
        //            try
        //            {
        //                await ctx.SaveChangesAsync();
        //                TempData[Constants.Message] = "Stožer ažuriran.";
        //                TempData[Constants.ErrorOccurred] = false;
        //                return RedirectToAction(nameof(Index), new { page, sort, ascending });
        //            }
        //            catch (Exception exc)
        //            {
        //                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        //                PrepareDropDownLists();
        //                return View(stozer);
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Podatke o stožeru nije moguće povezati s forme");
        //            PrepareDropDownLists();
        //            return View(stozer);
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        //        TempData[Constants.ErrorOccurred] = true;
        //        return RedirectToAction(nameof(Edit), id);
        //    }
        //}

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
                //vratiti prazan sadržaj?
                return PartialView("ErrorMessageRow", $"Neispravan id stozera: {id}");
            }
        }


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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Delete(int SifraStozera, int page = 1, int sort = 1, bool ascending = true)
        //{
        //    var stozer = ctx.Stozer.Find(SifraStozera);
        //    if (stozer == null)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        try
        //        {
        //            string naziv = stozer.Naziv;
        //            ctx.Remove(stozer);
        //            ctx.SaveChanges();
        //            TempData[Constants.Message] = $"Stožer {naziv} uspješno obrisan";
        //            TempData[Constants.ErrorOccurred] = false;
        //        }
        //        catch (Exception exc)
        //        {
        //            TempData[Constants.Message] = "Pogreška prilikom brisanja stožera: " + exc.CompleteExceptionMessage();
        //            TempData[Constants.ErrorOccurred] = true;
        //        }
        //        return RedirectToAction(nameof(Index), new { page, sort, ascending });
        //    }
        //}

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