using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Caching
{
    /// <summary>
    /// Wrapper around the caching mechanism provided by ASP.NET
    /// </summary>
    [RegisterInDependencyInjector(typeof(AspNetCacheWrapper), Singleton=true)]
    public interface ICacheProvider
    {
        /// <summary>
        /// Stores the specified object in the cache with the specified key,
        /// and with the specified life.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="life"></param>
        /// <param name="dependentKey">If not null, specifies the key of a dependent cached item.
        /// When the cached dependent item changes, "value" will be removed from the cache.
        /// This method creates the dependent key, with a value of String.Empty and
        /// no expiration, if it does not exist already.</param>
        void Set(string key, object value, TimeSpan life, string dependentKey = null);

        /// <summary>
        /// Obtains the object stored in the cache with the specified key,
        /// or null if there is no object stored with that key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);

        /// <summary>
        /// Removes from the cache the object stored with the specified key.
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);
    }
}
