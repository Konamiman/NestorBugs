using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement;
using Konamiman.NestorBugs.Web.Models;
using Konamiman.NestorBugs.CrossCutting.Caching;
using Konamiman.NestorBugs.Web.ApplicationLogic.Auxiliary;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement;
using Konamiman.NestorBugs.Data.Entities;
using Konamiman.NestorBugs.CrossCutting.Authorization;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement
{
    public class BugsOrchestrator : IBugsOrchestrator
    {
        private readonly ICacheProvider cacheProvider;
        private readonly IBugsManager bugsManager;
        private readonly TimeSpan bugCacheLife;
        private readonly IUserImageUrlProvider userImageUrlProvider;
        private readonly IServerAuthenticationEngineWrapper authenticationManager;

        public BugsOrchestrator(
            ICacheProvider cacheProvider,
            IBugsManager bugsManager,
            IConfigurationManager configurationManager,
            IUserImageUrlProvider userImageUrlProvider,
            IServerAuthenticationEngineWrapper authenticationManager)
        {
            this.cacheProvider = cacheProvider;
            this.bugsManager = bugsManager;
            this.userImageUrlProvider = userImageUrlProvider;
            this.authenticationManager = authenticationManager;

            int bugCacheLifeInMinutes = configurationManager.GetConfigurationValue<int>(ConfigurationKeys.UserDataCacheDurationMinutes);
            this.bugCacheLife = TimeSpan.FromMinutes(bugCacheLifeInMinutes);
        }

        #region Bug summaries retrieval

        public IEnumerable<BugSummaryViewModel> GetBugSummaries(int page, int pageSize, BugSortType sortType, string creatorUserName = null)
        {
            var bugIds = bugsManager.GetBugIds(page, pageSize, sortType, creatorUserName);

            var bugSummaries = new List<BugSummaryViewModel>();

            foreach(var bugId in bugIds) {
                var bugSummary = GetBugSummaryFromCache(bugId);
                if(bugSummary == null) {
                    bugSummary = GetBugSummaryFromDatabase(bugId);
                    InsertBugSummaryInCache(bugId, bugSummary);
                }
                bugSummaries.Add(bugSummary);
            }

            return bugSummaries.ToArray();
        }


        private BugSummaryViewModel GetBugSummaryFromCache(int bugId)
        {
            var cacheKey = GenerateCacheKeyForBugSummary(bugId);
            return (BugSummaryViewModel)cacheProvider.Get(cacheKey);
        }

        private BugSummaryViewModel GetBugSummaryFromDatabase(int bugId)
        {
            var bugSummary = bugsManager.GetBugSummary(bugId);
            bugSummary.UserImageUrl = userImageUrlProvider.GetUrlForUser(bugSummary.UserName, UserImageSizes.Small);
            return bugSummary;
        }

        private void InsertBugSummaryInCache(int bugId, BugSummaryViewModel bugSummary)
        {
            var cacheKey = GenerateCacheKeyForBugSummary(bugId);
            var dependentKey = string.Format(CacheKeys.BugDataChangedBase, bugId);
            cacheProvider.Set(cacheKey, bugSummary, bugCacheLife, dependentKey);
        }

        private string GenerateCacheKeyForBugSummary(int bugId)
        {
            return string.Format(CacheKeys.BugSummaryBase, bugId);
        }

        #endregion

        #region Bug details retrieval

        public BugDetailsViewModel GetBugByUrlTitle(string bugUrlTitle)
        {
            var bugDetailsViewModel = GetBugDetailsViewModelFromCache(bugUrlTitle);
            if(bugDetailsViewModel == null) {
                var bugDetails = bugsManager.GetBugByUrlTitle(bugUrlTitle);
                if(bugDetails == null) {
                    return null;
                }
                bugDetailsViewModel = bugsManager.CreateBugDetailsViewModel(bugDetails);
                InsertBugDetailsViewModelInCache(bugUrlTitle, bugDetailsViewModel);
            }
            bugsManager.SetCurrentUserDependentProperties(bugDetailsViewModel);

            return bugDetailsViewModel;
        }

        public BugDetailsViewModel GetNewBug()
        {
            var bugDetails = new Bug();
            var bugDetailsViewModel = bugsManager.CreateBugDetailsViewModel(bugDetails);
            return bugDetailsViewModel;
        }


        private BugDetailsViewModel GetBugDetailsViewModelFromCache(string bugUrlTitle)
        {
            var cacheKey = GenerateCacheKeyForBugDetails(bugUrlTitle);
            return (BugDetailsViewModel)cacheProvider.Get(cacheKey);
        }

        private string GenerateCacheKeyForBugDetails(string bugUrlTitle)
        {
            return string.Format(CacheKeys.BugDetailsBase, bugUrlTitle);
        }

        private void InsertBugDetailsViewModelInCache(string bugUrlTitle, BugDetailsViewModel bugDetailsViewModel)
        {
            var cacheKey = GenerateCacheKeyForBugDetails(bugUrlTitle);
            var dependentKey = string.Format(CacheKeys.BugDataChangedBase, bugUrlTitle);
            cacheProvider.Set(cacheKey, bugDetailsViewModel, bugCacheLife, dependentKey);
        }

        #endregion
    }
}