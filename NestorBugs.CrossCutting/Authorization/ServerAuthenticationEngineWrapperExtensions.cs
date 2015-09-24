using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Authorization
{
    public static class ServerAuthenticationEngineWrapperExtensions
    {
        public static bool CurrentUserIsSiteOwner(this IServerAuthenticationEngineWrapper authenticationEngine)
        {
            return authenticationEngine.IsUserAuthenticated() && authenticationEngine.IsUserInRole(RoleNames.Owner);
        }
    }
}
