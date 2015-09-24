using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Konamiman.NestorBugs.CrossCutting.Configuration
{
    public static class ConfigurationManagerExtensions
    {
        /// <summary>
        /// Obtains an application configuration value.
        /// </summary>
        /// <typeparam name="T">Type of the configuration value to obtain.</typeparam>
        /// <param name="configurationManager"></param>
        /// <param name="name">Name of the configuration value to obtain</param>
        /// <returns>Obtained configuration value</returns>
        /// <exception cref="System.ArgumentNullException">name is null</exception>
        /// <exception cref="System.ArgumentException">There is no configuration entry with the specified name</exception>
        /// <exception cref="System.InvalidCastException">The configuration value can't be converted to the specified type</exception>
        public static T GetConfigurationValue<T>(this IConfigurationManager configurationManager, string name)
        {
            var value = configurationManager.GetConfigurationValue(name);
            if(typeof(T) == typeof(string[]) && value.GetType() == typeof(StringCollection)) {
                var stringCollection = value as StringCollection;
                var stringArray = new string[stringCollection.Count];
                stringCollection.CopyTo(stringArray, 0);
                value = stringArray;
            }
            try {
                return (T)value;
            }
            catch(InvalidCastException ex) {
                throw new InvalidCastException(string.Format(
                    "Configuration value with key '{0}' is of type '{1}', which can't be converted to '{2}'",
                    name, value == null ? "(null)" : value.GetType().Name, typeof(T)),
                    ex);
            }
        }
    }
}
