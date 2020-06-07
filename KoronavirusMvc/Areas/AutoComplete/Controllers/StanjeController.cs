using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KoronavirusMvc.Areas.AutoComplete.Models;
using KoronavirusMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KoronavirusMvc.Areas.AutoComplete.Controllers
{
    [Area("AutoComplete")]
    public class StanjeController : Controller
    {
        private readonly RPPP09Context context;
        private readonly AppSettings appData;

        public StanjeController(RPPP09Context context, IOptionsSnapshot<AppSettings> options)
        {
            this.context = context;
            appData = options.Value;
        }

        public IEnumerable<IdLabel> Get(string term)
        {
            var query = context.Stanje
                               .Select(s => new IdLabel
                               {
                                   Id = s.SifraStanja,
                                   Label = s.NazivStanja
                               })
                               .Where(l => l.Label.Contains(term));
            var list = query.OrderBy(l => l.Label)
                            .Take(appData.AutoCompleteCount)
                            .ToList();
            return list;
        }
    }
}
