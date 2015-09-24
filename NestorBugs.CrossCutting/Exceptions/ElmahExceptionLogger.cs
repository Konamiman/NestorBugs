using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Exceptions
{
    public class ElmahExceptionLogger : IExceptionLogger
    {
        public void Log(Exception ex)
        {
            Elmah.ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Elmah.Error(ex));
        }
    }
}
