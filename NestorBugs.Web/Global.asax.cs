using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;
using Konamiman.NestorBugs.CrossCutting.Authorization;
using System.Reflection;
using DotNetOpenAuth.OpenId.RelyingParty;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using Konamiman.NestorBugs.Web;
using System.Configuration;
using Konamiman.NestorBugs.Web.Properties;
using System.Data.Entity;
using Konamiman.NestorBugs.Data.RepositoryContracts;
using Entities = Konamiman.NestorBugs.Data.Entities;
using System.IO;
using Konamiman.NestorBugs.Data.Tools;
using Konamiman.NestorBugs.Data;
using Konamiman.NestorBugs.CrossCutting.Misc;
using Konamiman.NestorBugs.CrossCutting.Exceptions;

namespace NestorBugs
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            IUnityContainer container = GetUnityContainer();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            IQueryableExtensions.ExceptionLogger = container.Resolve<IExceptionLogger>();

            DoDatabaseRelatedInitialization(container);
        }

        #region MVC engine initialization

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("admin/elmah.axd/{*pathInfo}");
            routes.IgnoreRoute("Home/{*action}");
            routes.IgnoreRoute("{*favicon}", new {favicon = @"(.*/)?favicon.ico(/.*)?"});

            routes.MapRoute(
                name: "Faq",
                url: "Faq",
                defaults: new
                {
                    controller = "Home", Action="Faq"
                }
            );

            routes.MapRoute(
                name: "Markdown",
                url: "Markdown",
                defaults: new
                {
                    controller = "Home", Action = "Markdown"
                }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home", action = "Index", id = (string)null
                }
            );
        }

        #endregion

        #region Dependency injector initialization

        private IUnityContainer GetUnityContainer()
        {
            IUnityContainer container = new UnityContainer();

            container.RegisterInterfacesWithRegistrationAttribute(
                new string[] {
                    Assembly.GetExecutingAssembly().FullName,
                    "NestorBugs.CrossCutting",
                    "NestorBugs.Data"
                });

            container
                .RegisterSingletonType<IControllerActivator, CustomControllerActivator>()
                .RegisterSingletonType<ApplicationSettingsBase, Settings>()
                ;

            return container;
        }

        #endregion

        #region Database related initialization

        private void DoDatabaseRelatedInitialization(IUnityContainer container)
        {
            var configurationManager = container.Resolve<IConfigurationManager>();

            var useFakeData =
                configurationManager.GetConfigurationValue<bool>(ConfigurationKeys.UseFakeData);

            string connectionString;
            if(useFakeData) {
                connectionString = GetFakeDataConnectionString(container, configurationManager);
                CreateFakeDatabaseAndDataIfNecessary(container, connectionString);
            }
            else {
                connectionString =
                    configurationManager.GetConfigurationValue<string>(ConfigurationKeys.ConnectionString)
                    .Replace("UserID", "user id");
            }

            container.RegisterSingletonInstance<string>("ConnectionString", connectionString);
        }

        private static string GetFakeDataConnectionString(IUnityContainer container, IConfigurationManager configurationManager)
        {
            string connectionString;
            var pathProvider = container.Resolve<ISpecialPathProvider>();
            var appDataPath = pathProvider.GetAppDataPath();
            connectionString =
                configurationManager.GetConfigurationValue<string>(ConfigurationKeys.FakeDataConnectionString)
                .Replace("|DataDirectory|", appDataPath);
            return connectionString;
        }

        private static void CreateFakeDatabaseAndDataIfNecessary(IUnityContainer container, string connectionString)
        {
            var DbContext = new NestorBugsEntities(connectionString);
            if(!DbContext.Database.Exists()) {
                DbContext.Database.Delete();
                try {
                    DbContext.Database.Create();
                    var fakeDataGenerator = container.Resolve<IFakeDataGenerator>();
                    fakeDataGenerator.FillWithFakeData(connectionString);
                }
                catch {
                    DbContext.Database.Delete();
                    throw;
                }
                finally {
                    DbContext.Dispose();
                }
            }
        }

        #endregion
    }
}