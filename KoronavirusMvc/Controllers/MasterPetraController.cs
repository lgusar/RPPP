using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Models;
using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        private async Task PrepareDropdownLists()
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
    }
}
