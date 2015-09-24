using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Konamiman.NestorBugs.CrossCutting.DependencyInjection
{
    // CODE FROM: http://weblogs.asp.net/shijuvarghese/archive/2011/01/21/dependency-injection-in-asp-net-mvc-3-using-dependencyresolver-and-controlleractivator.aspx
    
    /// <summary>
    /// This class is registered as the controller activator when the application starts.
    /// This forces the controller classes to be instantiated by using the dependency injection engine.
    /// </summary>
    public class CustomControllerActivator : IControllerActivator
    {
        IController IControllerActivator.Create(RequestContext requestContext, Type controllerType)
        {
            return DependencyResolver
                .Current
                .GetService(controllerType) as IController;
        }
    }
}
