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

namespace KoronavirusMvc.Controllers
{
    public class MasterPetraController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly AppSettings _appSettings;


        public MasterPetraController(RPPP09Context context, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }
        public async Task<IActionResult> Index()
        {
            var model = new MasterPetraViewModel();
            await PrepareDropdownLists();
            return View(model);
        }

        [HttpGet]
        public async Task PrepareDropdownLists()
        {
            var drzava = await _context.Drzava.OrderBy(d => d.ImeDrzave).Select(d => new { d.ImeDrzave, d.SifraDrzave }).ToListAsync();
            ViewBag.Drzave = new SelectList(drzava, nameof(Drzava.SifraDrzave), nameof(Drzava.ImeDrzave));
        }

        [HttpGet]
        public  async Task<IActionResult> GetLocationForCountry(string sifraDrzave) {
            var lokacije = await _context.Lokacija.Where(l => l.SifraDrzave == sifraDrzave.Trim()).ToListAsync();

            return PartialView("LokacijaMaster", lokacije);
        }

        [HttpGet]
        public async Task<IActionResult> GetContentForLocation(int sifraGrada)
        {
            var statistike = await _context.Statistika.Include(st => st.SifraOrganizacijeNavigation).Where(s => s.SifraGrada == sifraGrada).ToListAsync();
            var putovanja = await _context.PutovanjeLokacija.Include(pl => pl.SifraPutovanjaNavigation.IdentifikacijskiBrojNavigation).Where(s => s.SifraGrada == sifraGrada)
                .Select(pl => pl.SifraPutovanjaNavigation).ToListAsync();
            
            return PartialView("ContentMaster", (Statistike: statistike, Putovanja: putovanja ));
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePutovanje(int sifraPutovanja, int sifraGrada)
        {
            var putovanje = await _context.Putovanje.FirstAsync(p => p.SifraPutovanja == sifraPutovanja);
            _context.Remove(putovanje);
            await _context.SaveChangesAsync();

            return await GetContentForLocation(sifraGrada);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteStatistika(int sifraStatistike, int sifraGrada)
        {
            var statistika = await _context.Statistika.FirstAsync(p => p.SifraObjave == sifraStatistike);
            _context.Remove(statistika);
            await _context.SaveChangesAsync();

            return await GetContentForLocation(sifraGrada);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLocation(int sifraGrada, string sifraDrzave)
        {
            var location = await _context.Lokacija.FirstAsync(p => p.SifraGrada == sifraGrada);
            _context.Remove(location);
            await _context.SaveChangesAsync();

            return await GetLocationForCountry(sifraDrzave);
        }

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

        [HttpPost]
        public async Task<IActionResult> SaveStatistika(bool isAdd, int sifraStat, int brojIzlj, int brojBol, int brojUmrl, int brojTot, int sifraOrg, int sifraGrada)
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

            if (isAdd)
            {
                _context.Add(statistika);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }
    }
}
