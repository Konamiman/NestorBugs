using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.Data.RepositoryContracts;
using Konamiman.NestorBugs.CrossCutting.Exceptions;
using Konamiman.NestorBugs.CrossCutting.Misc;
using Konamiman.NestorBugs.Data.Entities;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository userRepository;
        private readonly IClock clock;

        public UserManager(IUserRepository userRepository, IClock clock)
        {
            this.userRepository = userRepository;
            this.clock = clock;
        }

        public bool CreateUserIfNotExists(string userName)
        {
            var existingUser = userRepository.GetUserByUserName(userName);
            if(existingUser != null) {
                return false;
            }

            var userId = userRepository.CreateNewUser(userName);

            var newUser = userRepository.GetUserByUserName(userName);
            if(newUser == null) {
                throw new DatabaseException(string.Format("UserRepository.CreateNewUser did not create the new user with username '{0}'", userName));
            }

            newUser.DisplayName = "user" + newUser.Id.ToString();
            newUser.JoinedDate = clock.UtcNow;
            userRepository.UpdateUser(newUser);

            return true;
        }


        public User GetUserByName(string userName)
        {
            return userRepository.GetUserByUserName(userName);
        }


        public bool UpdateUser(User user)
        {
            var userWithGivenDisplayName = userRepository.GetUserByDisplayName(user.DisplayName);
            if(userWithGivenDisplayName != null && userWithGivenDisplayName.Id != user.Id) {
                return false;
            }

            userRepository.UpdateUser(user);
            return true;
        }
    }
}
