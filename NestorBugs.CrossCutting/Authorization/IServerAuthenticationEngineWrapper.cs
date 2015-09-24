using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Authorization
{
    /// <summary>
    /// A wrapper for the membership engine provided by ASP.NET.
    /// </summary>
    [RegisterInDependencyInjector(typeof(FormsAuthenticationWrapper), Singleton = true)]
    public interface IServerAuthenticationEngineWrapper
    {
        /// <summary>
        /// Obtains the original URL requested by user, after a redirection to the login page.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        string GetRedirectUrl(string userName);

        /// <summary>
        /// Marks the specified user as signed in by generating the appropriate cookie.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="persistent"></param>
        void SignIn(string userName, bool persistent);

        /// <summary>
        /// Signs out the current signed in user.
        /// </summary>
        void SignOut();

        /// <summary>
        /// Checks if there is an authenticated user in the current session.
        /// </summary>
        /// <returns></returns>
        bool IsUserAuthenticated();

        /// <summary>
        /// Gets the name of the user currently logged in, null if no user is logged in.
        /// </summary>
        string CurrentUserName
        {
            get;
        }

        /// <summary>
        /// Checks if the current user is in the specified role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        bool IsUserInRole(string roleName);
    }
}
