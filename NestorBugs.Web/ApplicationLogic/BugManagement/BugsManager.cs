using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.Data;
using Konamiman.NestorBugs.Data.Entities;
using Konamiman.NestorBugs.Web.Models;
using Konamiman.NestorBugs.Data.Repositories;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using Microsoft.Practices.Unity;
using Konamiman.NestorBugs.CrossCutting.Authorization;
using Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement;
using Konamiman.NestorBugs.Web.ApplicationLogic.HtmlManipulation;
using System.Data;
using Konamiman.NestorBugs.CrossCutting.Misc;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement
{
    public class BugsManager : DbContextClientBase, IBugsManager
    {
        private readonly IServerAuthenticationEngineWrapper authenticationManager;
        private readonly IUserManager userManager;
        private readonly IMarkdownToHtmlConverter markdownConverter;
        private readonly IUserImageUrlProvider userImageUrlProvider;
        private readonly bool usingFakeData = false;

        [InjectionConstructor]
        public BugsManager(
            IConfigurationManager configurationManager,
            IUnityContainer unityContainer,
            IServerAuthenticationEngineWrapper authenticationManager,
            IUserManager userManager,
            IMarkdownToHtmlConverter markdownConverter,
            IUserImageUrlProvider userImageUrlProvider)
            : base(configurationManager, unityContainer)
        {
            this.authenticationManager = authenticationManager;
            this.userManager = userManager;
            this.markdownConverter = markdownConverter;
            this.userImageUrlProvider = userImageUrlProvider;

            usingFakeData =
                configurationManager.GetConfigurationValue<bool>(ConfigurationKeys.UseFakeData);
        }


        //This constructor is used for unit testing only
        public BugsManager(NestorBugsEntities dbContext)
            : base(dbContext)
        {
        }


        public int GetBugCount()
        {
            return ExecuteMethod("GetBugCount", () => _GetBugCount());
        }

        int _GetBugCount()
        {
            return DbContext.Bugs.Count();
        }


        public IEnumerable<int> GetBugIds(int page, int pageSize, BugSortType sortType, string creatorUserName = null)
        {
            return ExecuteMethod("GetBugIds", () => _GetBugIds(page, pageSize, sortType, creatorUserName));

        }

        IEnumerable<int> _GetBugIds(int page, int pageSize, BugSortType sortType, string creatorUserName = null)
        {
            var query = DbContext.Bugs.AsQueryable<Bug>();

            switch(sortType) {
                case BugSortType.Newest:
                    query = query.OrderByDescending(bug => bug.CreationDate);
                    break;
                case BugSortType.Recent:
                    query = query.OrderByDescending(bug => bug.LastEditDate);
                    break;
                case BugSortType.MostVoted:
                    query = query.OrderByDescending(bug => bug.Votes.Count());
                    break;
            }

            if(creatorUserName != null) {
                var x = DbContext.Users.Where(u => u.UserName == creatorUserName);
                var creator = DbContext.Users.Where(u => u.UserName == creatorUserName).SafeSingleOrDefault();
                if(creator == null) {
                    return new int[0];
                }
                query = query.Where(bug => bug.UserId == creator.Id);
            }

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return query.Select(bug => bug.Id).ToArray();
        }


        public int GetTotalBugCount(string creatorUserName = null)
        {
            return ExecuteMethod("GetTotalBugCount", () => _GetTotalBugCount(creatorUserName));
        }

        int _GetTotalBugCount(string creatorUserName = null)
        {
            if(creatorUserName == null) {
                return DbContext.Bugs.Count();
            }
            else {
                return DbContext.Bugs.Where(bug => bug.User.UserName == creatorUserName).Count();
            }
        }


        public BugSummaryViewModel GetBugSummary(int bugId)
        {
            return ExecuteMethod("GetBugSummary", () => _GetBugSummary(bugId));
        }

        BugSummaryViewModel _GetBugSummary(int bugId)
        {
            var bug = DbContext.Bugs.Where(b => b.Id == bugId).SafeSingleOrDefault();

            if(bug == null) {
                return null;
            }

            var bugSummaryViewModel = new BugSummaryViewModel()
            {
                ApplicationName = bug.Application == null ? bug.ApplicationName : bug.Application.Name,
                CommentCount = bug.Comments.Count(),
                CreationDate = bug.CreationDate,
                LastEditDate = bug.LastEditDate,
                Status = bug.Status,
                Title = bug.Title,
                UrlTitle = bug.UrlTitle,
                UserName = bug.User.DisplayName,
                UserUrl = bug.User.Url,
                VoteCount = bug.Votes.Count()
            };

            return bugSummaryViewModel;
        }


        public string GetBugUrlTitle(int bugId)
        {
            return ExecuteMethod("GetBugUrlTitle", () => _GetBugUrlTitle(bugId));
        }

        string _GetBugUrlTitle(int bugId)
        {
            return DbContext.Bugs
                .Where(bug => bug.Id == bugId)
                .Select(bug => bug.UrlTitle)
                .SafeSingleOrDefault();
        }


        public Bug GetBugByUrlTitle(string bugUrlTitle)
        {
            return ExecuteMethod("GetBugUrlTitle", () => _GetBugByUrlTitle(bugUrlTitle));
        }

        Bug _GetBugByUrlTitle(string bugUrlTitle)
        {
            var bug = DbContext.Bugs.Where(b => b.UrlTitle == bugUrlTitle).SafeSingleOrDefault();
            if(bug != null) {
                object dummy = bug.Application;
                dummy = bug.User;
            }
            return bug;
        }


        public Bug GetBugById(int id)
        {
            return ExecuteMethod("GetBugById", ()=> _GetBugById(id));
        }

        Bug _GetBugById(int id)
        {
            var bug = DbContext.Bugs.Where(b => b.Id == id).SafeSingleOrDefault();
            if(bug != null) {
                //TODO: Clean up this, user and app should be returned only when really needed.
                object dummy = bug.Application;
                dummy = bug.User;
            }
            return bug;
        }


        public BugDetailsViewModel CreateBugDetailsViewModel(Bug bugDetails)
        {
            return ExecuteMethod("CreateBugDetailsViewModel", () => _CreateBugDetailsViewModel(bugDetails));
        }

        BugDetailsViewModel _CreateBugDetailsViewModel(Bug bugDetails)
        {
            var bugVotesCount = DbContext.BugVotes.Where(vote => vote.BugId == bugDetails.Id).Count();
            var creator =  DbContext.Users.Where(u => u.Id == bugDetails.UserId).SafeSingle();

            var bugDetailsViewModel = new BugDetailsViewModel()
            {
                Bug = bugDetails,
                VotesCount = bugVotesCount,
                ApplicationName = bugDetails.Application == null ? bugDetails.ApplicationName : bugDetails.Application.Name,
                CreatorDisplayName = creator.DisplayName,
                BugDescriptionHtml = markdownConverter.ConvertMarkdownToHtml(bugDetails.Description),
                BugTestingEnvironmentHtml = markdownConverter.ConvertMarkdownToHtml(bugDetails.Environment),
                CreatorImageUrl = userImageUrlProvider.GetUrlForUser(creator.UserName, UserImageSizes.Small),
                CreatorUrl = creator.Url
            };

            if(bugDetails.LastEditUserId != null) {
                var lastEditor = DbContext.Users.Where(u => u.Id == bugDetails.LastEditUserId).SafeSingle();
                bugDetailsViewModel.LastEditorDisplayName = lastEditor.DisplayName;
                bugDetailsViewModel.LastEditorUrl = lastEditor.Url;
                bugDetailsViewModel.LastEditorImageUrl = userImageUrlProvider.GetUrlForUser(lastEditor.UserName, UserImageSizes.Small);
            }

            if(bugDetails.DuplicateBugId.HasValue) {
                var duplicateBug = DbContext.Bugs.Where(bug => bug.Id == bugDetails.DuplicateBugId).SafeSingleOrDefault();
                if(duplicateBug == null) {
                    throw new InvalidOperationException(string.Format(
                        "Bug with id {0} is marked as duplicate of bug with id {1}, but there is no bug with id {1} in the database.",
                        bugDetails.Id, bugDetails.DuplicateBugId.Value));
                }

                bugDetailsViewModel.DuplicateBugTitle = duplicateBug.Title;
                bugDetailsViewModel.DuplicateBugUrlTitle = duplicateBug.UrlTitle;
            }

            return bugDetailsViewModel;
        }


        public void SetCurrentUserDependentProperties(BugDetailsViewModel bugDetailsViewModel)
        {
            ExecuteMethod("SetCurrentUserDependentProperties", () => _SetCurrentUserDependentProperties(bugDetailsViewModel));
        }

        void _SetCurrentUserDependentProperties(BugDetailsViewModel bugDetailsViewModel)
        {
            int currentUserId = 0;
            bool currentUserIsSiteOwner = false;
            bool currentUserIsBugCreator = false;
            bool currentUserHasVotedBug = false;

            if(authenticationManager.IsUserAuthenticated()) {
                currentUserIsSiteOwner = authenticationManager.CurrentUserIsSiteOwner();

                if(bugDetailsViewModel.Bug.Id != 0) {
                    var currentUserName = authenticationManager.CurrentUserName;
                    currentUserId = userManager.GetUserByName(currentUserName).Id;

                    currentUserIsBugCreator = (bugDetailsViewModel.Bug.UserId == currentUserId);

                    var bugVoteId = DbContext
                        .BugVotes
                        .Where(vote => vote.BugId == bugDetailsViewModel.Bug.Id && vote.UserId == currentUserId)
                        .Select(vote => (int?)vote.Id)
                        .SafeSingleOrDefault();

                    currentUserHasVotedBug = bugVoteId.HasValue;
                }
            }

            bugDetailsViewModel.CurrentUserHasVotedThisBug = currentUserHasVotedBug;
            bugDetailsViewModel.CurrentUserIsBugCreator = currentUserIsBugCreator;
            bugDetailsViewModel.CurrentUserIsSiteOwner = currentUserIsSiteOwner;
            bugDetailsViewModel.UserIsLoggedIn = (currentUserId != 0);
        }


        public IEnumerable<Application> GetApplications()
        {
            return ExecuteMethod("GetApplications", () => _GetApplications());
        }

        IEnumerable<Application> _GetApplications()
        {
            return DbContext.Applications.ToArray();
        }


        public bool UserHasSubmittedBugs(string userName)
        {
            return ExecuteMethod("UserHasSubmittedBugs", ()=>_UserHasSubmittedBugs(userName));
        }

        public bool _UserHasSubmittedBugs(string userName)
        {
            return DbContext.Bugs.Where(bug => bug.User.UserName == userName).Count() > 0;
        }


        public void CreateOrUpdateBug(Bug bug)
        {
            ExecuteMethod("CreateOrUpdateBug", ()=>_CreateOrUpdateBug(bug));
        }

        void _CreateOrUpdateBug(Bug bug)
        {
            if(bug.Id == 0) {
                DbContext.Bugs.Add(bug);
            } else {
                var existingBug = DbContext.Entry<Bug>(bug);
                //TODO: This line is to avoid a "referential integrity constraint violation" error if app is changed. Find a better way.
                existingBug.Entity.Application = DbContext.Applications.Find(existingBug.Entity.ApplicationId);
                existingBug.State = EntityState.Modified;
            }

            DbContext.SaveChanges();

            if(usingFakeData && bug.Id == 0) {
                bug.Id = DbContext.Bugs.Select(b => b.Id).Max() + 1;
            }
        }


        public bool UserHasVotedBug(int bugId, int userId)
        {
            return ExecuteMethod("UserHasVotedBug", () => _UserHasVotedBug(bugId, userId));
        }

        bool _UserHasVotedBug(int bugId, int userId)
        {
            return UserHasVotedBugCore(bugId, userId);
        }


        bool UserHasVotedBugCore(int bugId, int userId)
        {
            return DbContext
                .BugVotes
                .Where(vote => vote.BugId == bugId && vote.UserId == userId)
                .Count() > 0;
        }


        public void CastBugVote(BugVote bugVote)
        {
            ExecuteMethod("CastBugVote", ()=>_CastBugVote(bugVote));
        }

        void _CastBugVote(BugVote bugVote)
        {
            if(!UserHasVotedBugCore(bugVote.BugId, bugVote.UserId)) {
                DbContext.BugVotes.Add(bugVote);
                DbContext.SaveChanges();
            }
        }


        public void RemoveBugVote(int bugId, int userId)
        {
            ExecuteMethod("RemoveBugVote", () => _RemoveBugVote(bugId, userId));
        }

        void _RemoveBugVote(int bugId, int userId)
        {
            var votes = DbContext.BugVotes.Where(vote => vote.BugId == bugId && vote.UserId == userId);
            foreach(var vote in votes) {
                DbContext.BugVotes.Remove(vote);
            }

            DbContext.SaveChanges();
        }


        public int GetBugVoteCount(int bugId)
        {
            return ExecuteMethod("GetBugVoteCount", () => _GetBugVoteCount(bugId));
        }

        int _GetBugVoteCount(int bugId)
        {
            var bug = DbContext.Bugs.Where(b => b.Id == bugId).SafeSingleOrDefault();
            if(bug == null) {
                return 0;
            }

            return bug.Votes.Count();
        }


        public string CreateBugEditToken(int bugId, int userId)
        {
            return ExecuteMethod("CreateBugEditToken", () => _CreateBugEditToken(bugId, userId));
        }

        string _CreateBugEditToken(int bugId, int userId)
        {
            var token = DbContext.BugEditTokens.Where(t => t.UserId == userId && t.BugId == bugId).SafeSingleOrDefault();
            if(token == null) {
                token = new BugEditToken()
                {
                    BugId = bugId,
                    UserId = userId,
                    Token = GenerateTokenString()
                };
                DbContext.BugEditTokens.Add(token);

                DbContext.SaveChanges();
            }
            return token.Token;
        }


        string GenerateTokenString()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }


        public string RetrieveBugEditToken(int bugId, int userId)
        {
            return ExecuteMethod("RetrieveBugEditToken", () => _RetrieveBugEditToken(bugId, userId));
        }

        string _RetrieveBugEditToken(int bugId, int userId)
        {
            return DbContext
                .BugEditTokens
                .Where(t => t.UserId == userId && t.BugId == bugId)
                .Select(t => t.Token)
                .SafeSingleOrDefault();
        }


        public void DeleteBugEditToken(int bugId, int userId)
        {
            ExecuteMethod("DeleteBugEditToken", ()=> _DeleteBugEditToken(bugId, userId));
        }

        void _DeleteBugEditToken(int bugId, int userId)
        {
            var token = DbContext.BugEditTokens.Where(t => t.UserId == userId && t.BugId == bugId).SafeSingleOrDefault();
            if(token != null) {
                DbContext.BugEditTokens.Remove(token);
                DbContext.SaveChanges();
            }
        }


        public bool ApplicationExists(int applicationId)
        {
            return ExecuteMethod("ApplicationExists", () => _ApplicationExists(applicationId));
        }
        
        bool _ApplicationExists(int applicationId)
        {
            return DbContext.Applications.Where(a => a.Id == applicationId).Count() > 0;
        }

    }
}