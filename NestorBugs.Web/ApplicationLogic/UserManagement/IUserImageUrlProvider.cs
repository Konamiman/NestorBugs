using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement
{
    /// <summary>
    /// Represents a class that obtains the URL for an image
    /// representing an user.
    /// </summary>
    [RegisterInDependencyInjector(typeof(GravatarUserImageProvider))]
    public interface IUserImageUrlProvider
    {
        string GetUrlForUser(string userName, int sizeInPixels);
    }
}