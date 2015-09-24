using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace Konamiman.NestorBugs.CrossCutting.Configuration
{
    public class SpecialPathProvider : ISpecialPathProvider
    {
        public string GetAppDataPath()
        {
            return HostingEnvironment.MapPath(@"~/App_Data");
        }
    }
}
