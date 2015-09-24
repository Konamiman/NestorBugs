using System;
using System.Web.Mvc;
using System.Web.Security;
using Konamiman.NestorBugs.CrossCutting.Authorization;
using Konamiman.NestorBugs.Web.Models;
using Microsoft.Practices.Unity;
using Konamiman.NestorBugs.CrossCutting.Misc;
using System.Globalization;
using System.Threading;
using Konamiman.NestorBugs.Data.Entities;
using System.ComponentModel;
using System.Web.Helpers;
using Konamiman.NestorBugs.CrossCutting.Caching;
using Entities = Konamiman.NestorBugs.Data.Entities;
using Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement;
using Konamiman.NestorBugs.Web.ApplicationLogic.Auxiliary;
using Konamiman.NestorBugs.CrossCutting.Configuration;

namespace Konamiman.NestorBugs.Web.Controllers
{
    /// <summary>
    /// Controller for all the user account related operations.
    /// </summary>
    public class AccountController : ControllerBase
    {
        private readonly IUnityContainer unityContainer;
        private readonly IServerAuthenticationEngineWrapper formsAuthentication;
        private readonly IUserManager userManager;
        private readonly IUrlNormalizer urlNormalizer;
        private readonly IUserViewModelProvider userViewModelProvider;
        private readonly ICacheProvider cacheProvider;
        private readonly bool usingFakeData;
        private readonly bool bypassOpenIdAuthentication;

        public AccountController(
            IUnityContainer unityContainer, 
            IServerAuthenticationEngineWrapper formsAuthentication,
            IUserManager userManager,
            IUrlNormalizer urlNormalizer,
            IUserViewModelProvider displayNameProvider,
            ICacheProvider cacheProvider,
            IConfigurationManager configurationManager)
        {
            this.unityContainer = unityContainer;
            this.formsAuthentication = formsAuthentication;
            this.userManager = userManager;
            this.urlNormalizer = urlNormalizer;
            this.userViewModelProvider = displayNameProvider;
            this.cacheProvider = cacheProvider;

            usingFakeData =
                configurationManager.GetConfigurationValue<bool>(ConfigurationKeys.UseFakeData);

            bypassOpenIdAuthentication =
                configurationManager.GetConfigurationValue<bool>(ConfigurationKeys.BypassOpenIdAuthentication);
        }

      
        /// <summary>
        /// This action is invoked when the user attempts to log on
        /// (via the "Log In" button or by requesting a page that requires authentication),
        /// and also when the OpenId server finishes the authentication process.
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOn()
        {
            var openId = unityContainer.Resolve<IOpenIdWrapper>();

            ViewBag.MainContentTitle = "Log On";
            switch(openId.State) {
                case OpenIdAuthenticationState.Initialized:
                    break;

                case OpenIdAuthenticationState.Authenticated:
                    var userName = urlNormalizer.GetNormalizedUrl(openId.UserSuppliedIdentifier);

                    string redirectUrl = openId.ReturnUrl;
                    if(redirectUrl == null) {
                        redirectUrl = formsAuthentication.GetRedirectUrl(userName);
                    }

                    var userCreated = userManager.CreateUserIfNotExists(userName);
                    formsAuthentication.SignIn(userName, persistent: openId.RememberMe);
                    if(userCreated) {
                        TempData["RedirectUrl"] = redirectUrl;
                        TempData["FirstTimeLogin"] = true;
                        return RedirectToAction("MyAccount");
                    }
                    else {
                        return Redirect(redirectUrl);
                    }

                case OpenIdAuthenticationState.Canceled:
                    ModelState.AddModelError("UserName", "Login was cancelled at the provider");
                    break;

                case OpenIdAuthenticationState.Failed:
                    ModelState.AddModelError("UserName", "Login failed using the provided OpenID identifier");
                    break;

                default:
                    throw new InvalidOperationException("OpenIdWrapper.State has an unknown value: " + openId.State.ToString());
            }

            ViewBag.ReturnUrl = Request.Params["ReturnUrl"];
            return View();
        }


        /// <summary>
        /// This action is invoked when the user hits the "Sign-In" button in the login page
        /// after he has selected a OpenId provider and has typed his username.
        /// </summary>
        /// <remarks>
        /// If the "UseFakeData" configuration setting is set to true,
        /// authentication is done by selecting the "OpenId" provider and
        /// directly typing the fake user name in the text box.
        /// All the OpenId processing is skipped.
        /// </remarks>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LogOn(LogOnModel model)
        {
            if(bypassOpenIdAuthentication) {
                return FakeLogOn(model);
            }

            var openId = unityContainer.Resolve<IOpenIdWrapper>();

            openId.UserSuppliedIdentifier = model.UserName;
            openId.RememberMe = model.RememberMe;
            openId.ReturnUrl = model.ReturnUrl;

            var redirect = openId.StartAuthenticationProcess();
            if(openId.State == OpenIdAuthenticationState.InvalidIdentifier) {
                ModelState.AddModelError("UserName", "Invalid OpenId identifier");
                return View(model);
            }
            else if(openId.State == OpenIdAuthenticationState.ProtocolError) {
                ModelState.AddModelError("UserName", "Invalid OpenId identifier or server unavailable");
                return View(model);
            }
            else if(redirect == null) {
                throw new InvalidOperationException("OpenIdWrapper.StartAuthenticationProcess returned null on state: " + openId.State.ToString());
            }
            else {
                return redirect;
            }
        }


        private ActionResult FakeLogOn(LogOnModel model)
        {
            var user = userManager.GetUserByName(model.UserName);
            if(user == null) {
                ModelState.AddModelError("UserName", "User not registered in database");
                return View(model);
            }

            formsAuthentication.SignIn(model.UserName, persistent: model.RememberMe);
            if(model.ReturnUrl == null) {
                return RedirectToMainPage();
            }
            else {
                return Redirect(model.ReturnUrl);
            }
        }


        /// <summary>
        /// User logout, invoked via the "Log Off" button.
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            formsAuthentication.SignOut();
            return RedirectToMainPage();
        }


        /// <summary>
        /// User account load, invoked via the "My Account" button
        /// and also automatically after login for new users.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult MyAccount()
        {
            ViewBag.MainContentTitle = "My Account";
            SetFirstTimeLoginViewBag();
            var userName = formsAuthentication.CurrentUserName;
            var user = userManager.GetUserByName(userName);
            TempData["RedirectUrl"] = TempData["RedirectUrl"];  //Needed to pass the value to the next action
            
            return View(user);
        }


        /// <summary>
        /// User account save changes, invoked via the "Save changes" button in the account details page.
        /// </summary>
        /// <param name="updatedUser"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken(Salt = "MyAccount")]
        public ActionResult MyAccount([Bind(Exclude = "Id, JoinedDate, UserName")]Entities.User updatedUser)
        {
            var userName = formsAuthentication.CurrentUserName;
            var user = userManager.GetUserByName(userName);
            ModelState.Remove("UserName"); //Needed to prevent "OpenId value is required" error

            if(TryUpdateModel(user, new string[] {"DisplayName", "Email", "Url"} )) {
                var updateOk = userManager.UpdateUser(user);
                if(updateOk) {
                    TempData["FirstTimeLogin"] = null;

                    ForceDeletionOfCachedUserData(userName);

                    var redirectUrl = TempData["RedirectUrl"] as string;
                    if(redirectUrl == null) {
                        return RedirectToMainPage();
                    }
                    else {
                        return Redirect(redirectUrl);
                    }
                } else {
                    ModelState.AddModelError("DisplayName", "This display name is already in use.");
                }
            }

            SetFirstTimeLoginViewBag();
            return View(user);
        }


        void ForceDeletionOfCachedUserData(string userName)
        {
            var key = string.Format(CacheKeys.UserDataChangedBase, userName);
            cacheProvider.Remove(key);
        }


        void SetFirstTimeLoginViewBag()
        {
            var firstTimeLogin = ((bool?)TempData["FirstTimeLogin"]).GetValueOrDefault();
            ViewBag.FirstTime = firstTimeLogin;
            TempData["FirstTimeLogin"] = TempData["FirstTimeLogin"];
        }
    }
}
