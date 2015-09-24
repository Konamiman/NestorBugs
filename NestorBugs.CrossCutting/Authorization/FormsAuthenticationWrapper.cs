using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web;

namespace Konamiman.NestorBugs.CrossCutting.Authorization
{
    public class FormsAuthenticationWrapper : IServerAuthenticationEngineWrapper
    {
        public string GetRedirectUrl(string userName)
        {
            //Note: the second parameter is ignored
            return FormsAuthentication.GetRedirectUrl(userName, false);
        }

        public void SignIn(string userName, bool persistent)
        {
            FormsAuthentication.SetAuthCookie(userName, persistent);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public bool IsUserAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public string CurrentUserName
        {
            get
            {
                return IsUserAuthenticated() ?
                    HttpContext.Current.User.Identity.Name :
                    null;
            }
        }

        public bool IsUserInRole(string roleName)
        {
            if(!IsUserAuthenticated()) {
                throw new InvalidOperationException("\"IsUserInRole\" can't be invoked when no user is signed in");
            }
            return Roles.IsUserInRole(roleName);
        }
    }
}
