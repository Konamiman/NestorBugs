using System;
using System.Data;
using System.Linq;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using Konamiman.NestorBugs.CrossCutting.Exceptions;
using Konamiman.NestorBugs.Data.Entities;
using Konamiman.NestorBugs.Data.RepositoryContracts;
using Konamiman.NestorBugs.CrossCutting.Misc;
using Microsoft.Practices.Unity;

namespace Konamiman.NestorBugs.Data.Repositories
{
    public class UserRepository : DbContextClientBase, IUserRepository
    {
        private readonly DateTime DefaultDate = new DateTime(2000, 1, 1);

        [InjectionConstructor]
        public UserRepository(
            IConfigurationManager configurationManager,
            IUnityContainer unityContainer)
            : base(configurationManager, unityContainer)
        {
        }


        public User GetUserByUserName(string userName)
        {
            return ExecuteMethod("GetUserByUserName", ()=> _GetUserByUserName(userName));
        }

        User _GetUserByUserName(string userName)
        {
            var user = DbContext.Users.Where(u => u.UserName == userName).SafeSingleOrDefault();
            return user;
        }


        public int CreateNewUser(string userName)
        {
            return ExecuteMethod("CreateNewUser", ()=> _CreateNewUser(userName));
        }

        int _CreateNewUser(string userName)
        {
            var user = new User()
            {
                UserName = userName,
                DisplayName = "user" + (new Random()).Next().ToString(),
                JoinedDate = DefaultDate
            };

            DbContext.Users.Add(user);

            DbContext.SaveChanges();

            return user.Id;
        }


        public void UpdateUser(User user)
        {
            ExecuteMethod("UpdateUser", ()=> _UpdateUser(user));
        }

        void _UpdateUser(User user)
        {
            var existingUser = DbContext.Entry<User>(user);
            existingUser.State = EntityState.Modified;
            DbContext.SaveChanges();
        }


        public User GetUserByDisplayName(string displayName)
        {
            return ExecuteMethod("GetUserByDisplayName", ()=> _GetUserByUserName(displayName));
        }

        User _GetUserByDisplayName(string displayName)
        {
            var user = DbContext.Users.Where(u => u.DisplayName == displayName).SafeSingleOrDefault();
            return user;
        }
    }
}
