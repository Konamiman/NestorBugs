using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.Data.Entities;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.Data.RepositoryContracts
{
    /// <summary>
    /// Repository for the application users.
    /// </summary>
    [RegisterInDependencyInjector("Konamiman.NestorBugs.Data.Repositories.UserRepository, NestorBugs.Data")]
    public interface IUserRepository
    {
        /// <summary>
        /// Obtains a user based on its user name, or null if there is no user with such name.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        User GetUserByUserName(string userName);

        /// <summary>
        /// Creates a new entry in the users database with the specified name,
        /// returns the id of the new user.
        /// </summary>
        /// <param name="userName"></param>
        int CreateNewUser(string userName);

        /// <summary>
        /// Updates user information in the database.
        /// </summary>
        /// <param name="user">New user information</param>
        void UpdateUser(User user);

        /// <summary>
        /// Gets the user that has the given display name, or null if there is none.
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        /// <exception cref="Konamiman.NestorBugs.CrossCutting.Exceptions.DatabaseException">There are two or more users in the database with the given display name.
        /// </exception>
        User GetUserByDisplayName(string displayName);
    }
}
