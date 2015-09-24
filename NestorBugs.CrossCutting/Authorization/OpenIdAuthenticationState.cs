using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Authorization
{
    /// <summary>
    /// State of an IOpenIdWrapper object
    /// </summary>
    public enum OpenIdAuthenticationState
    {
        /// <summary>
        /// No login in process
        /// </summary>
        Initialized,

        /// <summary>
        /// The supplied OpenId identifier is not valid
        /// </summary>
        InvalidIdentifier,

        /// <summary>
        /// Error when submitting authentication request to the server
        /// </summary>
        ProtocolError,

        /// <summary>
        /// User authenticated successfully
        /// </summary>
        Authenticated,

        /// <summary>
        /// Login was cancelled at the provider
        /// </summary>
        Canceled,

        /// <summary>
        /// Login failed
        /// </summary>
        Failed
    }
}
