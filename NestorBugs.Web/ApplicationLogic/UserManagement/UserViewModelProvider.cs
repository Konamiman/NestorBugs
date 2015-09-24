using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.CrossCutting.Caching;
using Konamiman.NestorBugs.CrossCutting.Authorization;
using Konamiman.NestorBugs.Web.Models;
using Konamiman.NestorBugs.Web.ApplicationLogic.Auxiliary;
using Konamiman.NestorBugs.CrossCutting.Configuration;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement
{
    public class UserViewModelProvider : IUserViewModelProvider
    {
        readonly ICacheProvider cacheProvider;
        readonly IUserManager userManager;
        readonly IUserImageUrlProvider userImageProvider;
        readonly TimeSpan cachedDataLife;


        public UserViewModelProvider(
            ICacheProvider cacheProvider,
            IUserManager userManager,
            IUserImageUrlProvider userImageProvider,
            IConfigurationManager configurationManager)
        {
            this.cacheProvider = cacheProvider;
            this.userManager = userManager;
            this.userImageProvider = userImageProvider;

            int cachedDataLifeInMinutes = configurationManager.GetConfigurationValue<int>(ConfigurationKeys.UserDataCacheDurationMinutes);
            this.cachedDataLife = TimeSpan.FromMinutes(cachedDataLifeInMinutes);
        }


        public UserViewModel GetUserViewModel(string userName)
        {
            if(userName == null) {
                throw new ArgumentNullException("UserViewModelProvider.GetUserViewModel: parameter userName can't be null");
            }

            var userViewModel = GetCachedUserViewModel(userName);
            if(userViewModel == null) {
                userViewModel = GenerateUserViewModel(userName);
                if(userViewModel != null) {
                    InsertUserViewModelInCache(userName, userViewModel);
                }
            }

            return userViewModel;
        }


        private void InsertUserViewModelInCache(string userName, object userViewModel)
        {
            var cacheKey = string.Format(CacheKeys.UserViewModelBase, userName);
            var dependentKey = string.Format(CacheKeys.UserDataChangedBase, userName);
            cacheProvider.Set(cacheKey, userViewModel, cachedDataLife, dependentKey);
        }


        private UserViewModel GenerateUserViewModel(string userName)
        {
            var userData = userManager.GetUserByName(userName);

            if(userData == null) {
                return null;
            }

            var userViewModel = new UserViewModel()
            {
                DisplayName = userData.DisplayName,
                ImageUrlSmall = userImageProvider.GetUrlForUser(userName, UserImageSizes.Small),
                ImageUrlBig = userImageProvider.GetUrlForUser(userName, UserImageSizes.Big)
            };

            return userViewModel;
        }


        private UserViewModel GetCachedUserViewModel(string userName)
        {
            var cacheKey = string.Format(CacheKeys.UserViewModelBase, userName);
            var userViewModel = (UserViewModel)cacheProvider.Get(cacheKey);
            return userViewModel;
        }
    }
}