#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System.Reflection;
using Konamiman.NestorBugs.CrossCutting.Exceptions;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Konamiman.NestorBugs.Data.Repositories
{
    /// <summary>
    /// This attribute should be applied to the methods of all classes derived from DbContextClientBase
    /// that need to use an instance of NestorBugsEntities. The instance is available in the
    /// protected property DbContext.
    /// </summary>
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Method, TargetMemberAttributes = MulticastAttributes.Instance)]
    [AttributeUsage(AttributeTargets.Method)]
    public class UsesDbContextAttribute : MethodInterceptionAspect
    {
        string methodName;
        MethodInfo createDataContextMethod;
        MethodInfo disposeDataContextMethod;

        public override bool CompileTimeValidate(MethodBase method)
        {
            var myClassName = this.GetType().Name;

            if(!typeof(DbContextClientBase).IsAssignableFrom(method.DeclaringType)) {
                Message.Write(SeverityType.Error, "ERROR",
                               "Cannot apply [{0}] to method {1} because it is not a member of a type " +
                               "derived from DbContextClientBase.", myClassName, method);
                return false;
            }

            this.methodName = method.Name;
            this.createDataContextMethod = method.DeclaringType.GetMethod("CreateDataContext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            this.disposeDataContextMethod = method.DeclaringType.GetMethod("DisposeDataContext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Message.Write(SeverityType.Info, "INFO", "Appliying [{0}] to method {1} of class {2}", 
                myClassName, method, method.DeclaringType);

            return true;
        }

        public override sealed void OnInvoke(MethodInterceptionArgs args)
        {
            createDataContextMethod.Invoke(args.Instance, null);

            try {
                args.Proceed();
            }
            catch(SqlException ex) {
                var message = string.Format("Database error when executing method '{0}' on class '{1}': {2}",
                    methodName, this.GetType().Name, ex.Message);
                throw new DatabaseException(message);
            }
            finally {
                disposeDataContextMethod.Invoke(args.Instance, null);
            }
        }
    }
}

#endif