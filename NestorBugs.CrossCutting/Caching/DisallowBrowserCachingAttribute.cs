using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Konamiman.NestorBugs.CrossCutting.Caching
{
    public class DisallowBrowserCachingAttribute : OutputCacheAttribute
    {
        public DisallowBrowserCachingAttribute()
            : base()
        {
            this.NoStore = true;
            this.Duration = 0;
            this.VaryByParam = "*";
        }
    }
}
