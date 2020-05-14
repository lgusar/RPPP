using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoronavirusMvc.Extensions
{
    public static class ExceptionExtensions
    {
        public static string CompleteExceptionMessage(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            while ( ex != null)
            {
                sb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }
    }
}
