using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using System.Web.Mvc;
using System.Configuration.Provider;
using Konamiman.NestorBugs.CrossCutting.Misc;

namespace Konamiman.NestorBugs.CrossCutting.Authorization
{
    public class CustomRoleProvider : RoleProvider
    {
        private readonly IConfigurationManager configurationManager;
        private readonly IUrlNormalizer urlNormalizer;
        private readonly bool bypassOpenIdAuthentication;

        public CustomRoleProvider()
        {
            this.configurationManager = DependencyResolver.Current.GetService<IConfigurationManager>();
            this.urlNormalizer = DependencyResolver.Current.GetService<IUrlNormalizer>();

            var configurationManager = DependencyResolver.Current.GetService<IConfigurationManager>();
            bypassOpenIdAuthentication =
                configurationManager.GetConfigurationValue<bool>(ConfigurationKeys.BypassOpenIdAuthentication);
        }

        //This constructor is never called by the membership framework,
        //it is intended for use in unit testing.
        public CustomRoleProvider(IConfigurationManager configurationManager, IUrlNormalizer urlNormalizer)
        {
            this.configurationManager = configurationManager;
            this.urlNormalizer = urlNormalizer;
        }

        public override string[] GetRolesForUser(string username)
        {
            if(string.IsNullOrWhiteSpace(username)) {
                throw new ProviderException("username can't be null nor an empty string");
            }

            if(bypassOpenIdAuthentication) {
                return RolesForFakeUser(username);
            }

            var roles = 
                urlNormalizer.GetNormalizedUrl(username) == configurationManager.GetConfigurationValue<string>(ConfigurationKeys.SiteOwnerOpenId)
                ? new string[] { RoleNames.Owner } : new string[0];
            return roles;
        }

        private string[] RolesForFakeUser(string username)
        {
            var roles =
                username == "user1"
                ? new string[] { RoleNames.Owner } : new string[0];
            return roles;
        }

        #region Not implemented members

        public override bool IsUserInRole(string username, string roleName)
        {
#if true
            throw new NotImplementedException();
#else
            if(string.IsNullOrWhiteSpace(username)) {
                throw new ProviderException("username can't be null nor an empty string");
            }
            if(string.IsNullOrWhiteSpace(username)) {
                throw new ProviderException("roleName can't be null nor an empty string");
            }

            if(roleName != RoleNames.Owner) {
                return false;
            }

            return Normalize(username) == configurationManager.GetConfigurationValue<string>(ConfigurationKeys.SiteOwnerOpenId);
#endif
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
