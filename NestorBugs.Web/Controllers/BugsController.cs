using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Konamiman.NestorBugs.CrossCutting.Authorization;
using Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement;
using Konamiman.NestorBugs.Data.Entities;
using Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement;
using Konamiman.NestorBugs.CrossCutting.Misc;
using System.Text;
using System.Globalization;
using Konamiman.NestorBugs.CrossCutting.Caching;
using Konamiman.NestorBugs.Web.ApplicationLogic.Auxiliary;
using System.Web.Security;
using System.Threading;
using Konamiman.NestorBugs.CrossCutting.Exceptions;
using System.Collections.Specialized;

namespace Konamiman.NestorBugs.Web.Controllers
{
    /// <summary>
    /// Controller for all the bug listing, viewving and editing operations.
    /// </summary>
    [DisallowBrowserCaching]
    public class BugsController : ControllerBase
    {
        private readonly IServerAuthenticationEngineWrapper serverAuthentication;
        private readonly IBugsOrchestrator bugsOrchestrator;
        private readonly IBugsManager bugsManager;
        private readonly IUserManager userManager;
        private readonly IClock clock;
        private readonly ICacheProvider cache;
        private readonly IExceptionLogger logger;

        const int AllApplications = 0;
        const int OtherApplications = -1;
        const string FilterCookieName = "BugsFilter";

        private const int pageSize = 30;

        public BugsController(
            IServerAuthenticationEngineWrapper serverAuthentication,
            IBugsOrchestrator bugsOrchestrator,
            IBugsManager bugsManager,
            IUserManager userManager,
            IClock clock,
            ICacheProvider cache,
            IExceptionLogger logger)
        {
            this.serverAuthentication = serverAuthentication;
            this.bugsOrchestrator = bugsOrchestrator;
            this.bugsManager = bugsManager;
            this.userManager = userManager;
            this.clock = clock;
            this.cache = cache;
            this.logger = logger;
        }


        /// <summary>
        /// Shows the list of bugs created by the current user.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        [ActionName("Mine")]
        public ActionResult CurrentUserBugList(int page = 1)
        {
            return BugsList("mine", page);
        }

       
        /// <summary>
        /// Shows the list of all existing bugs, sorted according the the "sort" parameter.
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [ActionName("List")]
        public ActionResult AllUsersBugsList(string sort = "recent", int page = 1)
        {
            if(sort == "mine") {
                return RedirectToActionPermanent("Mine", new { page = page });
            } else {
                return BugsList(sort, page);
            }
        }

        private ActionResult BugsList(string sort, int page)
        {
            string userName = null;
            if(sort == "mine") {
                userName = serverAuthentication.CurrentUserName;
            }

            if(page < 0) {
                page = 1;
            }

            var bugsCount = bugsManager.GetTotalBugCount(userName);
            var pageCount = CalculatePageCount(bugsCount);
            if(pageCount != 0 && page > pageCount) {
                page = pageCount;
            }
            ViewBag.PageCount = pageCount;

            BugSortType sortType;

            switch(sort) {
                case "recent":
                    ViewBag.MainContentTitle = "Recent bugs";
                    sortType = BugSortType.Recent;
                    break;
                case "newest":
                    ViewBag.MainContentTitle = "Newest bugs";
                    sortType = BugSortType.Newest;
                    break;
                case "mostvoted":
                    ViewBag.MainContentTitle = "Most voted bugs";
                    sortType = BugSortType.MostVoted;
                    break;
                case "mine":
                    ViewBag.MainContentTitle = "My bugs";
                    sortType = BugSortType.Recent;
                    break;
                default:
                    return RedirectToMainPage();
            }

            ViewBag.SortName = sort;
            ViewBag.Page = page;

            var bugSummaries = bugsOrchestrator.GetBugSummaries(page, pageSize, sortType, userName);
            return View("Bugs", bugSummaries);
        }

        private int CalculatePageCount(int bugsCount)
        {
            return (int)Math.Ceiling((decimal)bugsCount / pageSize);
        }


        /// <summary>
        /// Shows the details of a bug with the given id.
        /// The id can be the numeric id or the URL friendly title.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("View")]
        public ActionResult ViewBug(string id)  //Can't name the method "View" - shadows base member
        {
            string bugUrlTitle = TryConvertBugIdToUrlTitle(id);
            if(bugUrlTitle == null) {
                return NotFound();
            }
            else if(bugUrlTitle != id) {
                return RedirectToActionPermanent(null, new { id = bugUrlTitle });
            }

            var bugDetailsViewModel = bugsOrchestrator.GetBugByUrlTitle(bugUrlTitle);
            if(bugDetailsViewModel == null) {
                return NotFound();
            }

            ViewBag.MainContentTitle = "Bug details";
            ViewBag.MainContentTitleRight = "bug id: " + bugDetailsViewModel.Bug.Id;
            return View(bugDetailsViewModel);
        }


        /// <summary>
        /// Shows the bug edit page for a new bug.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult New()
        {
            var bug = new Bug() { Status = (byte)BugStatus.Proposed };
            ViewBag.MainContentTitle = "New bug";
            ViewBag.IsFirstBugForUser = !bugsManager.UserHasSubmittedBugs(serverAuthentication.CurrentUserName);
            return GetEditBugView(bug);
        }


        /// <summary>
        /// Shows the bug edit page for an existing bug.
        /// The id can be the numeric id or the URL friendly title.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult Edit(string id)
        {
            string bugUrlTitle = TryConvertBugIdToUrlTitle(id);
            if(bugUrlTitle == null) {
                return NotFound();
            }
            else if(bugUrlTitle != id) {
                return RedirectToActionPermanent(null, new { id = bugUrlTitle });
            }

            var bug = bugsManager.GetBugByUrlTitle(bugUrlTitle);
            if(bug == null) {
                return NotFound();
            }

            if(!CurrentUserCanEditBug(bug)) {
                return UnauthorizedToEdit();
            }

            if(bug.Locked && !serverAuthentication.CurrentUserIsSiteOwner()) {
                return UnauthorizedToEdit();
            }

            //TODO: Too much ViewBag. A ViewModel should be created.

            var user = userManager.GetUserByName(serverAuthentication.CurrentUserName);

            ViewBag.MainContentTitleRight = "bug id: " + bug.Id;
            ViewBag.MainContentTitle = "Edit bug";
            ViewBag.IsFirstBugForUser = false;
            string token = ViewBag.IdToken = bugsManager.CreateBugEditToken(bug.Id, user.Id);
            return GetEditBugView(bug);
        }


        [Authorize]
        [HttpPost]
        public ActionResult New(Bug submittedBug)
        {
            return CreateOrUpdateBug(submittedBug);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Edit(Bug submittedBug, string idToken)
        {
            return CreateOrUpdateBug(submittedBug, idToken);
        }


        ActionResult CreateOrUpdateBug(Bug submittedBug, string idToken = null)
        {
            //TODO: Split in smaller methods.

            var isNewBug = (submittedBug.Id == 0);
            Bug existingBug = null;
            string oldUrlTitle = null;
            var currentUser = userManager.GetUserByName(serverAuthentication.CurrentUserName);


            // Check for data tampering attempts

            if(isNewBug) {
                if(idToken != null) {
                    return UnauthorizedToEdit();
                }
                existingBug = new Bug() {
                    Status = (byte)BugStatus.Proposed
                };
            } else {
                existingBug = bugsManager.GetBugById(submittedBug.Id);
                if(existingBug == null) {
                    return UnauthorizedToEdit();
                }

                if(!CurrentUserCanEditBug(existingBug)) {
                    return UnauthorizedToEdit();
                }

                var existingBugIdToken = bugsManager.RetrieveBugEditToken(existingBug.Id, currentUser.Id);
                if(existingBugIdToken != idToken) {
                    return UnauthorizedToEdit();
                }

                oldUrlTitle = existingBug.UrlTitle;
            }

            if(existingBug.Locked && !serverAuthentication.CurrentUserIsSiteOwner()) {
                return UnauthorizedToEdit();
            }

            if(!isNewBug && !Enum.IsDefined(typeof(BugStatus), (int)submittedBug.Status)) {
                return UnauthorizedToEdit();
            }

            if(submittedBug.ApplicationId != -1 && !bugsManager.ApplicationExists(submittedBug.ApplicationId.Value)) {
                return UnauthorizedToEdit();
            }


            // Update existing (or new) model object

            var propertiesToUpdate = new List<string>() {
                "Title",
                "Description",
                "Environment",
                "ApplicationId",
                "ApplicationName",
                "ApplicationVersion"
            };
            if(serverAuthentication.CurrentUserIsSiteOwner()) {
                propertiesToUpdate.Add("Status");
                propertiesToUpdate.Add("Locked");
                propertiesToUpdate.Add("DuplicateBugId");
            }

            ModelState.Clear();
            if(existingBug.UrlTitle == null) {
                existingBug.UrlTitle = "dummy"; //Needed to avoid UpdateModel adding an error for UrlTitle.
            }
            TryUpdateModel<Bug>(existingBug, propertiesToUpdate.ToArray());

            existingBug.Title = existingBug.Title.Trim();
            

            // Perform additional data validation

            if(existingBug.DuplicateBugId != null) {
                var duplicateBug = bugsManager.GetBugById(existingBug.DuplicateBugId.Value);
                if(duplicateBug == null) {
                    ModelState.AddModelError("DuplicateBugId", "There is no bug with this id in database.");
                }
            }

            if(existingBug.ApplicationId == -1) {
                if(existingBug.ApplicationName == null) {
                    ModelState.AddModelError("ApplicationName", "When the selected application is \"Other\", the application name is needed.");
                }
                else {
                    existingBug.ApplicationId = null;
                }
            }
            else {
                existingBug.ApplicationName = null;
            }

            string newUrlTitle = null;
            if(existingBug.Title != null) {
                if(IsAllDigits(existingBug.Title)) {
                    ModelState.AddModelError("Title", "The title can't consist of only numbers.");
                    return GetEditBugView(submittedBug);
                }

                newUrlTitle = CalculateUrlTitle(existingBug.Title);
                if(newUrlTitle == "" || IsAllHyphens(newUrlTitle)) {
                    ModelState.AddModelError("Title", "The title must contain at least one ASCII letter.");
                }
                else {
                    var bugWithThisUrlTitle = bugsManager.GetBugByUrlTitle(newUrlTitle);
                    if(isNewBug && bugWithThisUrlTitle != null) {
                        ModelState.AddModelError("Title", "Another bug exists with the same or equivalent title.");
                    }
                    else if(!isNewBug && bugWithThisUrlTitle != null && bugWithThisUrlTitle.Id != existingBug.Id) {
                        ModelState.AddModelError("Title", "Another bug exists with the same or equivalent title.");
                    }
                }
            }

            if(!ModelState.IsValid) {
                return GetEditBugView(submittedBug);
            }


            // Wrap it up and save

            existingBug.UrlTitle = newUrlTitle;

            if(isNewBug) {
                existingBug.UserId = currentUser.Id;
                existingBug.CreationDate = clock.UtcNow;
            }
            else {
                existingBug.LastEditUserId = currentUser.Id;
                existingBug.LastEditDate = clock.UtcNow;
            }

            try {
                bugsManager.CreateOrUpdateBug(existingBug);
            } catch(Exception ex) {
                ModelState.AddModelError(string.Empty, "Server error. The bug could not be saved.");
                logger.Log(ex);

                return GetEditBugView(submittedBug);
            }

            if(!isNewBug) {
                ForceDeletionOfCachedBugData(existingBug, oldUrlTitle);
            }

            bugsManager.DeleteBugEditToken(existingBug.Id, currentUser.Id);
            return RedirectToAction("View", new { id = existingBug.UrlTitle });
        }


        private void ForceDeletionOfCachedBugData(Bug bug, string oldUrlTitle)
        {
            var key = string.Format(CacheKeys.BugDataChangedBase, bug.Id);
            cache.Remove(key);

            key = string.Format(CacheKeys.BugDataChangedBase, oldUrlTitle);
            cache.Remove(key);
        }


        bool IsAllDigits(string value)
        {
            return value.All(ch => char.IsDigit(ch) || ch == ' ');
        }


        bool IsAllHyphens(string value)
        {
            return value.All(ch => ch == '-');
        }


        string CalculateUrlTitle(string title)
        {
            title = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(title.Trim()));

            var urlTitle = new string(title.Where(ch => char.IsLetter(ch) || char.IsDigit(ch) || ch == ' ' || ch == '-').ToArray());

            return urlTitle.Replace(' ', '-').ToLower();
        }


        ActionResult UnauthorizedToEdit()
        {
            return new HttpStatusCodeResult(403, "You are not authorized to edit this bug.");
        }

        bool CurrentUserCanEditBug(Bug bug)
        {
            if(serverAuthentication.CurrentUserIsSiteOwner()) {
                return true;
            }

            var user = userManager.GetUserByName(serverAuthentication.CurrentUserName);
            if(user != null && user.Id == bug.UserId) {
                return true;
            }

            return false;
        }


        ActionResult GetEditBugView(Bug bug)
        {
            ViewBag.Applications = CreateSelectListForApplications(bug);
            ViewBag.Statuses = CreateSelectListForBugStatuses(bug);
            ViewBag.UserIsSiteOwner = serverAuthentication.CurrentUserIsSiteOwner();

            return View("Edit", bug);
        }

        private SelectList CreateSelectListForBugStatuses(Bug bug)
        {
            var possibleStatuses = (int[])Enum.GetValues(typeof(BugStatus));
            var possibleStatusesForSelect = possibleStatuses.Select(val => new
            {
                Id = val, 
                Name = ((BugStatus)val).GetDisplayText()
            });

            return new SelectList(
                items: possibleStatusesForSelect,
                dataValueField: "Id",
                dataTextField: "Name",
                selectedValue: bug.Status);
        }


        private SelectList CreateSelectListForApplications(Bug bug)
        {
            var applications = bugsManager.GetApplications().OrderByDescending(app => app.Name).ToList();
            var other = new Application()
            {
                Id = -1,
                Name = "(Other)"
            };
            applications.Add(other);

            return new SelectList(
                items: applications,
                dataValueField: "Id",
                dataTextField: "Name",
                selectedValue: bug.Application == null ? other.Id : bug.ApplicationId);
        }


        private string TryConvertBugIdToUrlTitle(string id)
        {
            var numericId = TryGetBugIdAsNumber(id);
            if(numericId == null) {
                return id;
            }
            else {
                return bugsManager.GetBugUrlTitle(numericId.Value);
            }
        }


        private int? TryGetBugIdAsNumber(string id)
        {
            int value;
            if(int.TryParse(id, out value) && value > 0) {
                return value;
            }
            else {
                return null;
            }
        }


        [Authorize]
        [HttpPost]
        public ActionResult VoteBug(int bugId) 
        {
            var bug = bugsManager.GetBugById(bugId);
            if(bug == null) {
                return JsonFail();
            }

            if(bug.Locked) {
                return JsonFail();
            }

            var user = userManager.GetUserByName(serverAuthentication.CurrentUserName);
            var voted = false;
            if(bugsManager.UserHasVotedBug(bugId, user.Id)) {
                bugsManager.RemoveBugVote(bugId, user.Id);
            }
            else {
                var vote = new BugVote()
                {
                    BugId = bugId,
                    UserId = user.Id,
                    IssueDate = clock.UtcNow
                };
                bugsManager.CastBugVote(vote);
                voted = true;
            }

            ForceDeletionOfCachedBugData(bug, bug.UrlTitle);
            var voteCount = bugsManager.GetBugVoteCount(bugId);

            return Json(new { result = "ok", votes = voteCount, voted = voted }, JsonRequestBehavior.DenyGet);
        }

        ActionResult JsonFail()
        {
            return Json(new { result="fail" }, JsonRequestBehavior.DenyGet);
        }


        public ActionResult Fail()
        {
            throw new InvalidOperationException("Nooooo!!!");
        }


        //TODO: Perhaps cookie management should go in its own class.

        FilterCookieValues GetFilterCookieValues()
        {
            var cookie = HttpContext
                .Request
                .Cookies
                .Cast<HttpCookie>()
                .Where(c => c.Name == FilterCookieName)
                .SingleOrDefault();
            if(cookie == null || !IsValidFilterCookie(cookie)) {
                return new FilterCookieValues();
            }

            int applicationId = AllApplications;
            int.TryParse(cookie["ApplicationId"], out applicationId);
            if(applicationId != AllApplications 
                && applicationId != OtherApplications
                && !bugsManager.ApplicationExists(applicationId)) {
                    return new FilterCookieValues();
                }

            bool hideClosedBugs = false;
            bool.TryParse(cookie["HideClosedBugs"], out hideClosedBugs);

            return new FilterCookieValues()
            {
                ApplicationId = applicationId,
                HideClosedBugs = hideClosedBugs
            };
        }


        void SetFilterCookieValues(int applicationId, bool hideClosedBugs)
        {
            var cookie = new HttpCookie(name: FilterCookieName);
            cookie.Values.Add("ApplicadionId", applicationId.ToString());
            cookie.Values.Add("HideClosedBugs", hideClosedBugs.ToString());

            HttpContext.Response.SetCookie(cookie);
        }


        bool IsValidFilterCookie(HttpCookie cookie)
        {
            return cookie.HasKeys && cookie["ApplicationId"] != null && cookie["HideClosedBugs"] != null;
        }


        class FilterCookieValues
        {
            public FilterCookieValues()
            {
                ApplicationId = AllApplications;
                HideClosedBugs = false;
            }

            public int ApplicationId
            {
                get;
                set;
            }

            public bool HideClosedBugs
            {
                get;
                set;
            }
        }
    }
}
