using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.CrossCutting.Authorization
{
    /// <summary>
    /// A wrapper for the authentication workflow performed by the DotNetOpenId class.
    /// </summary>
    [RegisterInDependencyInjector(typeof(DotNetOpenIdWrapper))]
    public interface IOpenIdWrapper
    {
        string UserSuppliedIdentifier
        {
            get;
            set;
        }

        OpenIdAuthenticationState State
        {
            get;
        }

        bool RememberMe
        {
            get;
            set;
        }

        string ReturnUrl
        {
            get;
            set;
        }

        ActionResult StartAuthenticationProcess();

        string UserEmail
        {
            get;
        }

    }
}
