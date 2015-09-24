using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Reflection;
using System.IO;

namespace Konamiman.NestorBugs.CrossCutting.DependencyInjection
{
    public static class UnityExtensions
    {
        #region RegisterInterfacesWithRegistrationAttribute

        /// <summary>
        /// This method will search for all the interfaces with the "RegisterInDependencyInjector" attribute,
        /// and will register them in the unity container. Interfaces are searched in the current assembly
        /// and in all the referenced assemblies that are already loaded; assemblies that do not meet dependentAssemblyFilter
        /// (applied to assembly full names) are filtered out
        /// (default filter is "Assembly name must not start with System").
        /// </summary>
        /// <param name="container">Unity container in which interfaces will be registered.</param>
        /// <param name="baseAssembly">Base assembly in wich interfaces will be searched.
        /// Assemblies referenced by this assembly will be scanned too.</param>
        /// <param name="referencedAssembliesFullNameFilter">
        /// Condition that the base and referenced assemblies must meet in order to be included
        /// in the search for interfaces. If omitted or null, all assemblies whose name does not
        /// start with "System" or "mscorlib" will be scanned.
        /// </param>
        public static void RegisterInterfacesWithRegistrationAttribute(
           this IUnityContainer container,
           Assembly baseAssembly,
           Predicate<string> referencedAssembliesFullNameFilter = null)
        {
            if(referencedAssembliesFullNameFilter == null) {
                referencedAssembliesFullNameFilter = (name => !name.StartsWith("System") && !name.StartsWith("mscorlib"));
            }

            var assemblyNames = baseAssembly
                .GetReferencedAssemblies()
                .Union(new AssemblyName[] { baseAssembly.GetName() })
                .Where(a => referencedAssembliesFullNameFilter(a.FullName));

            RegisterInterfacesWithRegistrationAttribute(
                container,
                assemblyNames.Select(an => an.FullName));
        }


        /// <summary>
        /// This method will search for all the interfaces with the "RegisterInDependencyInjector" attribute,
        /// and will register them in the unity container. Interfaces are searched in the assemblies
        /// with the specified names, which are first loaded if necessary.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblyNames">Names of the assemblies where interfaces will be searched for.
        /// The names can be either fully qualified names or simple names;
        /// in any case, they must be suitable for the Assembly.Load method.</param>
        public static void RegisterInterfacesWithRegistrationAttribute(
           this IUnityContainer container,
           IEnumerable<string> assemblyNames)
        {
            IEnumerable<Assembly> assemblies;

            if(assemblyNames.Any(name => string.IsNullOrWhiteSpace(name))) {
                throw new ArgumentException("The assembly names list can't contain null or empty strings");
            }

            assemblies = assemblyNames
                .Select((name, index) =>
                {
                    try {
                        return GetOrLoadAssembly(name);
                    }
                    catch(Exception ex) {
                        throw new InvalidOperationException(string.Format(
                            "{0} when loading assembly \"{1}\" for interface scanning. See InnerException for details.",
                            ex.GetType().Name, assemblyNames.Skip(index).First()), ex);
                    }
                });
            

            RegisterInterfacesWithRegistrationAttribute(
                container,
                assemblies);
        }

        private static Assembly GetOrLoadAssembly(string name)
        {
            var assembly = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName == name || a.GetName().Name == name).SingleOrDefault();
            if(assembly == null) {
                return Assembly.Load(name);
            }
            else {
                return assembly;
            }
        }


        /// <summary>
        /// This method will search for all the interfaces with the "RegisterInDependencyInjector" attribute,
        /// and will register them in the unity container. Interfaces are searched in the specified assemblies.
        /// </summary>
        /// <param name="container">Unity container in which interfaces will be registered.</param>
        /// <param name="assemblies">Assemblies where interfaces will be searched for.</param>
        public static void RegisterInterfacesWithRegistrationAttribute(
            this IUnityContainer container,
            IEnumerable<Assembly> assemblies) {

            foreach(var assembly in assemblies) {
                var interfaceTypes = assembly.GetTypes().Where(t => t.IsInterface && t.IsPublic);
                foreach(var interfaceType in interfaceTypes) {
                    var registerAttribute = GetRegisterAttribute(interfaceType);
                    if(registerAttribute != null) {
                        RegisterInterfaceBasedOnAttribute(container, assembly, interfaceType, registerAttribute);
                    }
                }
            }
        }


        private static void RegisterInterfaceBasedOnAttribute(
            IUnityContainer container, 
            Assembly assembly, 
            Type interfaceType, 
            RegisterInDependencyInjectorAttribute registerAttribute)
        {
            var classType = registerAttribute.MappedClassType;
            if(classType == null) {
                classType = GetClassTypeFromNameInRegisterAttribute(assembly, interfaceType, registerAttribute);
            }

            if(!interfaceType.IsAssignableFrom(classType)) {
                throw new InvalidOperationException(string.Format(
                        "Interface \"{0}\" has a RegisterInDependencyInjector attribute, but the specified mapped class \"{1}\" does not implement the interface.",
                        interfaceType.FullName, classType.FullName));
            }

            container.RegisterType(interfaceType, classType,
                registerAttribute.Singleton ?
                (LifetimeManager)new ContainerControlledLifetimeManager() :
                (LifetimeManager)null);
        }


        private static RegisterInDependencyInjectorAttribute GetRegisterAttribute(Type interfaceType)
        {
            var registerAttribute = interfaceType
                .GetCustomAttributes(typeof(RegisterInDependencyInjectorAttribute), false)
                .SingleOrDefault() as RegisterInDependencyInjectorAttribute;
            return registerAttribute;
        }


        private static Type GetClassTypeFromNameInRegisterAttribute(
            Assembly assembly, 
            Type interfaceType, 
            RegisterInDependencyInjectorAttribute registerAttribute)
        {
            var className = registerAttribute.MappedClassName;
            if(className == null) {
                if(!interfaceType.Name.StartsWith("I")) {
                    throw new InvalidOperationException(string.Format(
                        "Interface \"{0}\" has a RegisterInDependencyInjector attribute with no mapped class name. This is allowed only for interfaces whose name starts with \"I\".",
                        interfaceType.FullName));
                }
                className = interfaceType.Namespace + "." + interfaceType.Name.Substring(1);
            }
            else if(!className.Contains(".") && !className.Contains(",")) {
                className = interfaceType.Namespace + "." + className;
            }
            var classType = assembly.GetType(className, throwOnError: false);
            if(classType == null) {
                classType = Type.GetType(className, throwOnError: false);
            }

            if(classType == null) {
                throw new InvalidOperationException(string.Format(
                    "Interface \"{0}\" has a RegisterInDependencyInjector attribute, but the (perhaps implicit) specified mapped type name \"{1}\" can't be found",
                    interfaceType.FullName, className));
            }
            return classType;
        }

        #endregion


        public static IUnityContainer RegisterSingletonType<TFrom, TTo>(
            this IUnityContainer container,
            params InjectionMember[] injectionMembers)
                where TTo : TFrom
        {
            container.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager(), injectionMembers);
            return container;
        }


        public static IUnityContainer RegisterSingletonType<T>(
            this IUnityContainer container,
            params InjectionMember[] injectionMembers)
        {
            container.RegisterType<T>(new ContainerControlledLifetimeManager(), injectionMembers);
            return container;
        }


        public static IUnityContainer RegisterSingletonInstance<T>(
            this IUnityContainer container,
            string name,
            T instance)
        {
            container.RegisterInstance<T>(name, instance, new ContainerControlledLifetimeManager());
            return container;
        }
    }
}
