using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;

namespace Konamiman.NestorBugs.CrossCutting.Caching
{
    class AspNetCacheWrapper : ICacheProvider
    {
        string[] cacheKeyDependency = new string[1];

        public void Set(string key, object value, TimeSpan life, string dependentKey = null)
        {
            if(dependentKey != null) {
                cacheKeyDependency[0] = dependentKey;
                if(HttpContext.Current.Cache[dependentKey] == null) {
                    HttpContext.Current.Cache.Insert(dependentKey, string.Empty);
                }
            }

            HttpContext.Current.Cache.Insert(
                key,
                value,
                dependencies: dependentKey == null ? null : new CacheDependency(null, cacheKeyDependency),
                absoluteExpiration: Cache.NoAbsoluteExpiration,
                slidingExpiration: life
            );
        }

        public object Get(string key)
        {
            return HttpContext.Current.Cache[key];
        }

        public void Remove(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }
    }
}
