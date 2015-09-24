using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Threading;
using System.Web.Routing;

namespace Konamiman.NestorBugs.Web.Controllers
{
    public abstract class ControllerBase : Controller
    {
        string requestUrl;
        string referrerUrl;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            const string culture = "en-US";
            CultureInfo ci = CultureInfo.GetCultureInfo(culture);

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        public RedirectToRouteResult RedirectToMainPage()
        {
            return RedirectToAction(string.Empty, string.Empty);
        }

        /// <summary>
        /// Returning this action from a controller method
        /// causes the "Not found" page to be displayed
        /// (returning "HttpNotFound" results on the browser saying "Link is broken")
        /// </summary>
        /// <returns></returns>
        public ActionResult NotFound()
        {
            ViewBag.RequestedUrl = requestUrl;
            ViewBag.ReferrerUrl = referrerUrl;
            return View("NotFound");
        }

        //This is needed by the NotFound method
        protected override void Execute(RequestContext requestContext)
        {
            requestUrl = requestContext.HttpContext.Request.Url.ToString();

            var referrerUrlObject = requestContext.HttpContext.Request.UrlReferrer;
            referrerUrl = referrerUrlObject == null ? null : referrerUrlObject.AbsoluteUri;

            base.Execute(requestContext);
        }
    }
}
