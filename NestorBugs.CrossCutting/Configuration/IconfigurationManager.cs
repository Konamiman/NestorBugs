using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Configuration
{
    /// <summary>
    /// Represents a class that allows obtaining application-wide configuration values.
    /// </summary>
    [RegisterInDependencyInjector(typeof(SettingsFileBasedConfigurationManager))]
    public interface IConfigurationManager
    {
        /// <summary>
        /// Obtains an application configuration value.
        /// </summary>
        /// <param name="name">Name of the configuration value to obtain</param>
        /// <returns>Obtained configuration value</returns>
        /// <exception cref="System.ArgumentNullException">name is null</exception>
        /// <exception cref="System.ArgumentException">There is no configuration entry with the specified name</exception>
        object GetConfigurationValue(string name);

        /// <summary>
        /// Checks if a configuration value with the specified name exists.
        /// </summary>
        /// <param name="name">Name of the configuration value to check</param>
        /// <returns>True if there is a configuration value with the specified name, false otehrwise</returns>
        bool ConfigurationValueExists(string name);
    }
}
