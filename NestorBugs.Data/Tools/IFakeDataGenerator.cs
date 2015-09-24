using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.Data.Tools
{
    [RegisterInDependencyInjector(Singleton = true)]
    public interface IFakeDataGenerator
    {
        void FillWithFakeData(string connectionString);
    }
}
