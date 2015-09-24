using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;

namespace Konamiman.NestorBugs.CrossCutting.Configuration
{
    /// <summary>
    /// Configuration manager that obtains the values from the contents of a .settings file
    /// (values can be configured with the .settings file itself or in the Web.config file).
    /// </summary>
    public class SettingsFileBasedConfigurationManager : IConfigurationManager
    {
        readonly ApplicationSettingsBase settingsProvider;
        Dictionary<string, object> configValues = null;

        public SettingsFileBasedConfigurationManager(ApplicationSettingsBase settingsProvider)
        {
            this.settingsProvider = settingsProvider;
        }


        public bool ConfigurationValueExists(string name)
        {
            InitializeApplicationSettingsIfNecesary();
            return configValues.ContainsKey(name);
        }


        public object GetConfigurationValue(string name)
        {
            InitializeApplicationSettingsIfNecesary();
            if(name == null) {
                throw new ArgumentNullException("Configuration key can't be null");
            }
            if(!configValues.ContainsKey(name)) {
                throw new ArgumentException(string.Format("Configuration key '{0}' does not exist", name));
            }
            return configValues[name];
        }


        private void InitializeApplicationSettingsIfNecesary()
        {
            if(configValues == null) {
                var applicationScopedSettingsProperties = settingsProvider
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttributes(typeof(ApplicationScopedSettingAttribute), false).Count() > 0);

                configValues = applicationScopedSettingsProperties.ToDictionary<PropertyInfo, string, object>(
                    p => p.Name,
                    p => p.GetValue(settingsProvider, null));
            }
        }
      
    }
}
