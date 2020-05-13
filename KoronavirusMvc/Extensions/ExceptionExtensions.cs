using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoronavirusMvc.Extensions
{
    /// <summary>
    /// Razred sa proširenjem za iznimke
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Vraća sadržaj poruka cijele hijerarhije neke iznimke. Za predanu iznimku provjerava se postoji li unutarnja iznimka.
        /// Ako da, poruka unutarnje iznimke dodaje se u rezultat te se dalje provjerava postoji li unutarnja iznimka unutarnje iznlimke itd...
        /// </summary>
        /// <param name="exc"></param>
        /// <returns></returns>
        public static string CompleteExceptionMessage(this Exception exc)
        {
            StringBuilder sb = new StringBuilder();
            while(exc != null)
            {
                sb.AppendLine(exc.Message);
                exc = exc.InnerException;
            }
            return sb.ToString();
        }
    }
}
