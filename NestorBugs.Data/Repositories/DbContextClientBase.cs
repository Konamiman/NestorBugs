using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using System.Data.SqlClient;
using Konamiman.NestorBugs.CrossCutting.Exceptions;
using Konamiman.NestorBugs.Data.Tools;
using Microsoft.Practices.Unity;
using System.IO;

namespace Konamiman.NestorBugs.Data.Repositories
{
    /// <summary>
    /// Base class for all the classes that make use of the solution DbContext (NestorBugsEntities).
    /// </summary>
    public abstract class DbContextClientBase
    {
        private readonly string connectionString;
        private readonly bool useTestingRepository = false;
        private readonly IFakeDataGenerator fakeDataGenerator = null;
        private readonly string databasePath = null;

        public DbContextClientBase(
            IConfigurationManager configurationManager,
            IUnityContainer unityContainer)
        {
            this.connectionString =
                unityContainer.Resolve<string>("ConnectionString");
        }

        //This constructor is used for unit testing only
        public DbContextClientBase(NestorBugsEntities dbContext)
        {
            this.DbContext = dbContext;
            useTestingRepository = true;
        }

        //Used by UsesDbContextAttribute
        protected void CreateDataContext()
        {
            this.DbContext = 
                useTestingRepository ?
                DbContext :
                new NestorBugsEntities(connectionString);
        }

        //Used by UsesDbContextAttribute
        protected void DisposeDataContext()
        {
            if(DbContext != null) {
                DbContext.Dispose();
                DbContext = null;
            }
        }

        protected NestorBugsEntities DbContext
        {
            get;
            private set;
        }


        // ExecuteMethod is not used anymore
        // (UsesDbContextAttribute is used instead)
#if true
        /// <summary>
        /// Executes a method by wrapping it in a code block that instantiates the DbContext
        /// and performs the appropriate error handling.
        /// </summary>
        /// <typeparam name="T">Type of the value returned by the executed method.</typeparam>
        /// <param name="methodName">Name of the invoker, used for logging only.</param>
        /// <param name="method">Delegate with the code to be executed.</param>
        /// <returns>value returned by the executed method.</returns>
        /// <exception cref="System.Data.SqlClient.SqlException">Error when accessing the database</exception>
        protected T ExecuteMethod<T>(string methodName, Func<T> method)
        {
            CreateDataContext();
            try {
                return method();
            }
            catch(SqlException ex) {
                var message = string.Format("Database error when executing method '{0}' on class '{1}': {2}",
                    methodName, this.GetType().Name, ex.Message);
                throw new DatabaseException(message, ex);
            }
            finally {
                DbContext.Dispose();
            }
        }


        /// <summary>
        /// Executes a method by wrapping it in a code block that instantiates the DbContext
        /// and performs the appropriate error handling.
        /// </summary>
        /// <param name="methodName">>Name of the invoker, used for logging only.</param>
        /// <param name="method">Delegate with the code to be executed.</param>
        protected void ExecuteMethod(string methodName, Action method)
        {
            ExecuteMethod<object>(methodName, () =>
            {
                method();
                return null;
            });
        }
#endif
    }
}
