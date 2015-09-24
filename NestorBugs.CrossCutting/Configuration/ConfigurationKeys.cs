using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Configuration
{
    /// <summary>
    /// Contains the names of all the configuration values that exist in the application Settings.settings file.
    /// </summary>
    public static class ConfigurationKeys
    {
        public static readonly string SiteOwnerOpenId = "SiteOwnerOpenId";
        public static readonly string ConnectionString = "ConnectionString";
        public static readonly string UserDataCacheDurationMinutes = "UserDataCacheDurationMinutes";
        public static readonly string BugDataCacheDurationInMinutes = "BugDataCacheDurationInMinutes";
        public static readonly string UseFakeData = "UseFakeData";
        public static readonly string FakeBugsCount = "FakeBugsCount";
        public static readonly string FakeUsersCount = "FakeUsersCount";
        public static readonly string BypassOpenIdAuthentication = "BypassOpenIdAuthentication";
        public static readonly string FakeDataConnectionString = "FakeDataConnectionString";
    }
}
