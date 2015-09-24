using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Configuration
{
    [RegisterInDependencyInjector(Singleton=true)]
    public interface ISpecialPathProvider
    {
        string GetAppDataPath();
    }
}
