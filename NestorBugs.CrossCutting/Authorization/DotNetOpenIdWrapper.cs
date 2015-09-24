using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

namespace Konamiman.NestorBugs.CrossCutting.Authorization
{
    //CAUTION: Do NOT reigster this class as singleton!

    public class DotNetOpenIdWrapper : IOpenIdWrapper
    {
        OpenIdRelyingParty openIdManager;

        public DotNetOpenIdWrapper() 
        {
            openIdManager = new OpenIdRelyingParty();
            IAuthenticationResponse response = openIdManager.GetResponse();
            if(response == null) {
                UserSuppliedIdentifier = null;
                ReturnUrl = null;
                State = OpenIdAuthenticationState.Initialized;
            } else {
                UserSuppliedIdentifier = response.ClaimedIdentifier;
                switch(response.Status) {
                    case AuthenticationStatus.Authenticated:
                        var callbackArguments = response.GetCallbackArguments();
                        RememberMe = Convert.ToBoolean(callbackArguments["NestorBugs.RememberMe"]);
                        if(callbackArguments.ContainsKey("NestorBugs.ReturnUrl")) {
                            ReturnUrl = callbackArguments["NestorBugs.ReturnUrl"];
                        }
                        else {
                            ReturnUrl = null;
                        }

                        ClaimsResponse claims = response.GetExtension(typeof(ClaimsResponse)) as ClaimsResponse;
                        if(claims != null) {
                            UserEmail = claims.Email;
                        }
                        else {
                            UserEmail = null;
                        }

                        State = OpenIdAuthenticationState.Authenticated;
                        break;

                    case AuthenticationStatus.Canceled:
                        State = OpenIdAuthenticationState.Canceled;
                        break;

                    case AuthenticationStatus.Failed:
                        State = OpenIdAuthenticationState.Failed;
                        break;

                    default:
                        throw new InvalidOperationException("OpenIdRelyingParty.Response.Status has an unknown value: " + response.Status);
                }
            }
        }


        public string UserSuppliedIdentifier
        {
            get;
            set;
        }

        public OpenIdAuthenticationState State
        {
            get; private set;
        }

        public bool RememberMe
        {
            get; set;
        }

        public string ReturnUrl
        {
            get;
            set;
        }

        public ActionResult StartAuthenticationProcess()
        {
            if(!Identifier.IsValid(UserSuppliedIdentifier)) {
                State = OpenIdAuthenticationState.InvalidIdentifier;
                return null;
            }

            IAuthenticationRequest request = null;
            try {
                request = openIdManager.CreateRequest(Identifier.Parse(UserSuppliedIdentifier));
            }
            catch(ProtocolException) {
                State = OpenIdAuthenticationState.ProtocolError;
                return null;
            }

            var callbackArguments = new Dictionary<string, string>() {
                { "NestorBugs.RememberMe", Convert.ToString(RememberMe) }
            };
            if(ReturnUrl != null) {
                callbackArguments.Add("NestorBugs.ReturnUrl", ReturnUrl);
            }
            request.AddCallbackArguments(callbackArguments);

            //var fetch = new FetchRequest();
            //fetch.Attributes.AddRequired(WellKnownAttributes.Name.FullName);
            //request.AddExtension(fetch);

            request.AddExtension(new ClaimsRequest()
            {
                Email = DemandLevel.Require,
            });

            return request.RedirectingResponse.AsActionResult();
        }

        public string UserEmail
        {
            get; private set;
        }
    }
}
