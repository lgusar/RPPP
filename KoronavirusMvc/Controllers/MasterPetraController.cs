using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace KoronavirusMvc.Controllers
{
    public class MasterPetraController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly AppSettings _appSettings;


        /// <summary>
        /// stvaranje konteksta apliakcije
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appSettings"></param>
        public MasterPetraController(RPPP09Context context, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// funkcija se ne poziva, prije se koristila za stranicenje
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var model = new MasterPetraViewModel();
            await PrepareDropdownLists();
            return View(model);
        }

        /// <summary>
        /// stvaranje i dobivanje padajuce liste stranih kljuceva
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task PrepareDropdownLists()
        {
            var drzava = await _context.Drzava.OrderBy(d => d.ImeDrzave).Select(d => new { d.ImeDrzave, d.SifraDrzave }).ToListAsync();
            ViewBag.Drzave = new SelectList(drzava, nameof(Drzava.SifraDrzave), nameof(Drzava.ImeDrzave));
        }

        /// <summary>
        /// dobivanje lsvih lokacija od neke odabrane drzave
        /// </summary>
        /// <param name="sifraDrzave"></param>
        /// <returns></returns>
        [HttpGet]
        public  async Task<IActionResult> GetLocationForCountry(string sifraDrzave) {
            var lokacije = await _context.Lokacija.Where(l => l.SifraDrzave == sifraDrzave.Trim()).ToListAsync();

            return PartialView("LokacijaMaster", lokacije);
        }

        /// <summary>
        /// vracanje svih statistika iz odabranog grada kao i putovanja koja su se tamo odvila
        /// </summary>
        /// <param name="sifraGrada"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetContentForLocation(int sifraGrada)
        {
            var statistike = await _context.Statistika.Include(st => st.SifraOrganizacijeNavigation).Where(s => s.SifraGrada == sifraGrada).ToListAsync();
            var putovanjaIds = _context.PutovanjeLokacija.Where(s => s.SifraGrada == sifraGrada)
                .Select(pl => pl.SifraPutovanja);
            var putovanja = await _context.Putovanje.Include(p => p.IdentifikacijskiBrojNavigation).Join(putovanjaIds, p => p.SifraPutovanja, pi => pi, (p, pi) => p).ToListAsync();
            
            
            return PartialView("ContentMaster", (Statistike: statistike, Putovanja: putovanja ));
        }

        /// <summary>
        /// brisanje odabranog putovanja iz master detail forme
        /// </summary>
        /// <param name="sifraPutovanja"></param>
        /// <param name="sifraGrada"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeletePutovanje(int sifraPutovanja, int sifraGrada)
        {
            var putovanje = await _context.Putovanje.FirstAsync(p => p.SifraPutovanja == sifraPutovanja);
            _context.Remove(putovanje);
            await _context.SaveChangesAsync();

            return await GetContentForLocation(sifraGrada);
        }

        /// <summary>
        /// brisanje statistika iz master detail forme
        /// </summary>
        /// <param name="sifraStatistike"></param>
        /// <param name="sifraGrada"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteStatistika(int sifraStatistike, int sifraGrada)
        {
            var statistika = await _context.Statistika.FirstAsync(p => p.SifraObjave == sifraStatistike);
            _context.Remove(statistika);
            await _context.SaveChangesAsync();

            return await GetContentForLocation(sifraGrada);
        }

        /// <summary>
        /// brisanje gradova iz master detail forme
        /// </summary>
        /// <param name="sifraGrada"></param>
        /// <param name="sifraDrzave"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteLocation(int sifraGrada, string sifraDrzave)
        {
            var location = await _context.Lokacija.FirstAsync(p => p.SifraGrada == sifraGrada);
            _context.Remove(location);
            await _context.SaveChangesAsync();

            return await GetLocationForCountry(sifraDrzave);
        }

        /// <summary>
        /// dodavanje i editiranje drzava iz mastera
        /// </summary>
        /// <param name="sifraDrzave"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDrzavaAddEdit(string sifraDrzave)
        {
            Drzava drzava;
            if (!string.IsNullOrWhiteSpace(sifraDrzave))
            {
                drzava = await _context.Drzava.FirstAsync(d => d.SifraDrzave == sifraDrzave.Trim());
            }
            else
            {
                drzava = new Drzava();
            }

            return PartialView("DrzaveEditMaster", drzava);
        }

        /// <summary>
        /// spremanje drzave iz mastera
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="sifraDrzave"></param>
        /// <param name="imeDrzave"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveDrzava(bool isAdd, string sifraDrzave, string imeDrzave)
        {
            var drzava = await _context.Drzava.FirstOrDefaultAsync(g => g.SifraDrzave == sifraDrzave.Trim());
            if (isAdd)
            {
                if (drzava != null)
                {
                    return await GetDrzavaAddEdit(null);
                }
                else drzava = new Drzava
                {
                    SifraDrzave = sifraDrzave.Trim()
                };
            }

            drzava.ImeDrzave = imeDrzave;

            if (isAdd)
            {
                _context.Add(drzava);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        /// <summary>
        /// dodavanje i editiranje gradova iz mastera
        /// </summary>
        /// <param name="sifraGrada"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetLokacijaAddEdit(int sifraGrada)
        {
            Lokacija lokacija;
            if(sifraGrada > 0)
            {
                lokacija = await _context.Lokacija.FirstAsync(g => g.SifraGrada == sifraGrada);
            }
            else
            {
                lokacija = new Lokacija();
            }

            var drzave = await _context.Drzava.OrderBy(d => d.ImeDrzave).Select(d => new { d.ImeDrzave, d.SifraDrzave }).ToListAsync();

            return PartialView("LocationEditMaster", (Lokacija: lokacija, drzave: new SelectList(drzave, nameof(Drzava.SifraDrzave), nameof(Drzava.ImeDrzave))));
        }

        /// <summary>
        /// spremanje gradova
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="sifraGrada"></param>
        /// <param name="imeGrada"></param>
        /// <param name="sifraDrzave"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult>SaveLokacija(bool isAdd, int sifraGrada, string imeGrada, string sifraDrzave)
        {
            var lokacija = await _context.Lokacija.FirstOrDefaultAsync(g => g.SifraGrada == sifraGrada);
            if (isAdd)
            {
                if (lokacija != null)
                {
                    return await GetLokacijaAddEdit(0);
                }
                else lokacija = new Lokacija
                {
                    SifraGrada = sifraGrada
                };
            }

            lokacija.ImeGrada = imeGrada;
            lokacija.SifraDrzave = sifraDrzave;

            if (isAdd)
            {
                _context.Add(lokacija);
            }

            await _context.SaveChangesAsync();

           return Ok(new { Success = true });
        }

        /// <summary>
        /// dodavanje i editiranje putovanja iz mastera
        /// </summary>
        /// <param name="sifraPutovanja"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPutovanjeAddEdit(int sifraPutovanja)
        {
            Putovanje putovanje;
            if(sifraPutovanja > 0)
            {
                putovanje = await _context.Putovanje.FirstAsync(p => p.SifraPutovanja == sifraPutovanja);
            }
            else
            {
                putovanje = new Putovanje();
            }

            var lokacije = await _context.Lokacija.OrderBy(d => d.ImeGrada).Select(d => new { d.ImeGrada, d.SifraGrada }).ToListAsync();
            var osobe = await _context.Osoba.OrderBy(d => d.Ime).Select(d => new { d.Ime, d.IdentifikacijskiBroj }).ToListAsync();

            var viewModel = new MasterPutovanjeEditViewModel
            {
                Putovanje = putovanje,
                Lokacije = new SelectList(lokacije, nameof(Lokacija.SifraGrada), nameof(Lokacija.ImeGrada)),
                Osobe = new SelectList(osobe, nameof(Osoba.IdentifikacijskiBroj), nameof(Osoba.Ime))
            };

            return PartialView("PutovanjeEditMaster", viewModel);
        }


        /// <summary>
        /// spremanje putovanja
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="sifraPutovanja"></param>
        /// <param name="datumPolaska"></param>
        /// <param name="datumVracanja"></param>
        /// <param name="gradovi"></param>
        /// <param name="osoba"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SavePutovanje(bool isAdd, int sifraPutovanja, DateTime datumPolaska, DateTime datumVracanja, List<int> gradovi, string osoba)
        {
            var putovanje = await _context.Putovanje.FirstOrDefaultAsync(g => g.SifraPutovanja == sifraPutovanja);
            if (isAdd)
            {
                if (putovanje != null)
                {
                    return await GetPutovanjeAddEdit(0);
                }
                else putovanje = new Putovanje
                {
                    SifraPutovanja = sifraPutovanja
                };
            }

            putovanje.DatumPolaska = datumPolaska;
            putovanje.DatumVracanja = datumVracanja;
            putovanje.IdentifikacijskiBroj = osoba;

            if (isAdd)
            {
                _context.Add(putovanje);
            }

            foreach (var grad in gradovi)
            {
                _context.Add(new PutovanjeLokacija
                {
                    SifraPutovanja = putovanje.SifraPutovanja,
                    SifraGrada = grad
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        /// <summary>
        /// dodavanje i editiranje statistika iz mastera
        /// </summary>
        /// <param name="sifraStatistike"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetStatistikaAddEdit(int sifraStatistike)
        {
            Statistika statistika;
            if(sifraStatistike > 0)
            {
                statistika = await _context.Statistika.FirstAsync(s => s.SifraObjave == sifraStatistike);
            }
            else
            {
                statistika = new Statistika();
            }

            var lokacije = await _context.Lokacija.OrderBy(d => d.ImeGrada).Select(d => new { d.ImeGrada, d.SifraGrada }).ToListAsync();
            var organizacije = await _context.Organizacija.OrderBy(d => d.Naziv).Select(d => new { d.Naziv, d.SifraOrganizacije }).ToListAsync();

            var viewModel = new MasterStatistikaEditViewModel
            {
                Statistika = statistika,
                Lokacije = new SelectList(lokacije, nameof(Lokacija.SifraGrada), nameof(Lokacija.ImeGrada)),
                Organizacije = new SelectList(organizacije, nameof(Organizacija.SifraOrganizacije), nameof(Organizacija.Naziv))
            };

            return PartialView("StatistikaEditMaster", viewModel);
        }

        /// <summary>
        /// spreamnje statistike
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="sifraStat"></param>
        /// <param name="brojIzlj"></param>
        /// <param name="brojBol"></param>
        /// <param name="brojUmrl"></param>
        /// <param name="brojTot"></param>
        /// <param name="sifraOrg"></param>
        /// <param name="sifraGrada"></param>
        /// <param name="datum"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveStatistika(bool isAdd, int sifraStat, int brojIzlj, int brojBol, int brojUmrl, int brojTot, int sifraOrg, int sifraGrada, DateTime datum)
        {
            var statistika = await _context.Statistika.FirstOrDefaultAsync(g => g.SifraObjave == sifraStat);
            if (isAdd)
            {
                if (statistika != null)
                {
                    return await GetStatistikaAddEdit(0);
                }
                else statistika = new Statistika
                {
                    SifraObjave = sifraStat
                };
            }

            statistika.BrojIzlijecenih = brojIzlj;
            statistika.BrojAktivnih = brojBol;
            statistika.BrojSlucajeva = brojTot;
            statistika.BrojUmrlih = brojUmrl;
            statistika.SifraGrada = sifraGrada;
            statistika.SifraOrganizacije = sifraOrg;
            statistika.Datum = datum;

            if (isAdd)
            {
                _context.Add(statistika);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        /// <summary>
        /// dohvacanje drzava koje koje pocinju nekim stringom (term)
        /// za autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<object> Get(string term)
        {
            var query = _context.Lokacija.Where(l => l.ImeGrada.Contains(term))
                               .Select(s => new
                               {
                                   Id = s.SifraGrada,
                                   Label = s.ImeGrada.Trim()
                               });
            var list = query.OrderBy(l => l.Label)
                            .ToList();
            return list;
        }

        /// <summary>
        /// dohvacanje drzava
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<object> GetDrzava(string term)
        {
            var query = _context.Drzava.Where(l => l.ImeDrzave.StartsWith(term))
                               .Select(s => new
                               {
                                   value = s.SifraDrzave,
                                   label = s.ImeDrzave.Trim()
                               });
            var list = query.OrderBy(l => l.label)
                            .ToList();
            return list;
        }

        /// <summary>
        /// generiranje excela od mastera
        /// </summary>
        /// <param name="sifraDrzave"></param>
        public async void exportToExcelDetail(string sifraDrzave)
        {
            var drzava = await _context.Drzava.FirstAsync(d => d.SifraDrzave == sifraDrzave);

            List<Lokacija> lokacije = _context.Lokacija.Where( l => l.SifraDrzave == sifraDrzave).ToList();

            List<Statistika> statistike = new List<Statistika>();
            List<Putovanje> putovanja = new List<Putovanje>();
            foreach (var item in lokacije)
            {
                statistike.AddRange(_context.Statistika.Where(s => s.SifraGrada == item.SifraGrada).ToList());
                var dodajPutovanja = _context.PutovanjeLokacija.Where(s => s.SifraGrada == item.SifraGrada).ToList();
                foreach (var it in dodajPutovanja)
                {
                    putovanja.AddRange(_context.Putovanje.Where(p => p.SifraPutovanja == it.SifraPutovanja));
                }
            }


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add($"Drzava {drzava.SifraDrzave}");

            ws.Cells["A3"].Value = string.Format("Gradovi povezani sa državom:");
            int rowStart = 4;
            foreach (var item in lokacije)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.ImeGrada;
                rowStart++;
            }
            //ws.Cells["A1"].Value = $"Pregled {pregled.SifraPregleda}";

            //ws.Cells["A3"].Value = "Datum";
            //ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

            //ws.Cells["A6"].Value = "Sifra Pregleda";
            //ws.Cells["B6"].Value = "Datum";
            //ws.Cells["C6"].Value = "Anamneza";
            //ws.Cells["D6"].Value = "Dijagnoza";

            //ws.Cells["A7"].Value = pregled.SifraPregleda;
            //ws.Cells["B7"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", pregled.Datum);
            //ws.Cells["C7"].Value = pregled.Anamneza;
            //ws.Cells["D7"].Value = pregled.Dijagnoza;

            //ws.Cells["A10"].Value = "Simptomi";

            //rowStart = 11;
            //foreach (Simptom s in simptomi)
            //{
            //    ws.Cells[string.Format("A{0}", rowStart)].Value = s.Opis;
            //    rowStart++;
            //}

            //ws.Cells["C10"].Value = "Terapije";

            //rowStart = 11;
            //foreach (Terapija t in terapije)
            //{
            //    ws.Cells[string.Format("C{0}", rowStart)].Value = t.OpisTerapije;
            //    rowStart++;
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}
            //}

            //ws.Cells["A:AZ"].AutoFitColumns();
            //Response.Clear();
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.Headers.Add("content-disposition", $"attachment; filename=pregled{pregled.SifraPregleda}.xlsx");
            //Response.Body.WriteAsync(pck.GetAsByteArray());
            //Response.CompleteAsync();
        }
    }
}
