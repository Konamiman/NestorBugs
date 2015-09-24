using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

namespace Konamiman.NestorBugs.CrossCutting.DependencyInjection
{
    // CODE FROM: http://weblogs.asp.net/shijuvarghese/archive/2011/01/21/dependency-injection-in-asp-net-mvc-3-using-dependencyresolver-and-controlleractivator.aspx

    public class UnityDependencyResolver : IDependencyResolver
    {
        IUnityContainer container;
        public UnityDependencyResolver(IUnityContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            try {
                return container.Resolve(serviceType);
            }
            catch {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try {
                return container.ResolveAll(serviceType);
            }
            catch {
                return new List<object>();
            }
        }
    }
}
