using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Exceptions
{
    [RegisterInDependencyInjector(typeof(ElmahExceptionLogger), Singleton=true)]
    public interface IExceptionLogger
    {
        void Log(Exception ex);
    }
}
