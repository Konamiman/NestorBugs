using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.CrossCutting.Caching;
using System.Web.Security;
using Konamiman.NestorBugs.Data.RepositoryContracts;
using Konamiman.NestorBugs.CrossCutting.Misc;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using Konamiman.NestorBugs.Web.ApplicationLogic.Auxiliary;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement
{
    public class GravatarUserImageProvider : IUserImageUrlProvider
    {
        private readonly IUserRepository userRepository;
        private readonly IUrlNormalizer urlNormalizer;

        public GravatarUserImageProvider(
            IUserRepository userRepository,
            IUrlNormalizer urlNormalizer)
        {
            this.userRepository = userRepository;
            this.urlNormalizer = urlNormalizer;
        }

        public string GetUrlForUser(string userName, int sizeInPixels)
        {
            if(userName != null) {
                userName = urlNormalizer.GetNormalizedUrl(userName);
            }

            if(userName == null) {
                return GetGravatarUrlForAnonymousUser(sizeInPixels);
            }
            else {
                return GetGravatarUrlForKnwonUser(userName, sizeInPixels);
            }
        }

        private string GetGravatarUrlForAnonymousUser(int sizeInPixels)
        {
            return string.Format("http://www.gravatar.com/avatar?s={0}&d=mm&f=y", sizeInPixels);
        }

        private string GetGravatarUrlForKnwonUser(string userName, int sizeInPixels)
        {
            string url;
            var user = userRepository.GetUserByUserName(userName);
            var email = (user == null ? null : user.Email);
            if(email == null) {
                url = string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=retro&f=y", 
                    FormsAuthentication.HashPasswordForStoringInConfigFile(userName, "MD5"), sizeInPixels);
            }
            else {
                url = string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=retro",
                    FormsAuthentication.HashPasswordForStoringInConfigFile(email, "MD5"), sizeInPixels);
            }
            return url.ToLower();
        }
    }
}