using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;
using Konamiman.NestorBugs.Web.Models;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement
{
    /// <summary>
    /// This class retrieves UserViewModel objects, orchestrating the
    /// cache and database access as necessary.
    /// </summary>
    [RegisterInDependencyInjector(typeof(UserViewModelProvider))]
    public interface IUserViewModelProvider
    {
        UserViewModel GetUserViewModel(string userName);
    }
}