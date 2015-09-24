using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.DependencyInjection
{
    /// <summary>
    /// Marks an interface for registration in the dependency injection (Unity) container.
    /// See the RegisterInterfacesWithRegistrationAttribute method in the UnityExtensions class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class RegisterInDependencyInjectorAttribute : Attribute
    {
        /// <summary>
        /// Type of the class the interface will be mapped to.
        /// If null, property MappedClassName will be used instead.
        /// </summary>
        public Type MappedClassType
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the class the interface will be mapped to. If null,
        /// the class name is the same as the interface name without the initial "I",
        /// in the same assembly and namespace. If it is a simple name
        /// (with no namespace nor assembly name) then the same namespace
        /// and assembly of the interface is assumed.
        /// </summary>
        public string MappedClassName
        {
            get;
            set;
        }

        /// <summary>
        /// If true, the interface must be registered in single instance mode.
        /// </summary>
        public bool Singleton
        {
            get;
            set;
        }

        public RegisterInDependencyInjectorAttribute(Type mappedClassType)
            : this(mappedClassType, null)
        {
        }

        public RegisterInDependencyInjectorAttribute()
            : this(null, null)
        {
        }

        public RegisterInDependencyInjectorAttribute(string mappedClassName)
            : this(null, mappedClassName)
        {
        }

        private RegisterInDependencyInjectorAttribute(Type mappedClassType, string mappedClassName)
        {
            this.MappedClassType = mappedClassType;
            this.MappedClassName = mappedClassName;
        }
    }
}
