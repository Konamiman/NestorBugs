using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Misc
{
    [RegisterInDependencyInjector]
    public interface IClock
    {
        DateTime Now
        {
            get;
        }

        DateTime UtcNow
        {
            get;
        }
    }
}
