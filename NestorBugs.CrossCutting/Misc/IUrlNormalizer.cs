using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Misc
{
    /// <summary>
    /// Represents a class that provides a URL normalization service.
    /// </summary>
    [RegisterInDependencyInjector(typeof(UrlNormalizer), Singleton=true)]
    public interface IUrlNormalizer
    {
        string GetNormalizedUrl(string url);
    }
}
