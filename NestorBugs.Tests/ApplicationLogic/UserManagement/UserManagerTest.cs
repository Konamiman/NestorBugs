using Konamiman.NestorBugs.Web.ApplicationLogic.UserManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using Moq.Matchers;
using Konamiman.NestorBugs.Data.RepositoryContracts;
using Konamiman.NestorBugs.Data.Entities;
using Konamiman.NestorBugs.CrossCutting.Misc;

namespace NestorBugs.Tests.ApplicationLogic
{
    
    
    /// <summary>
    ///Se trata de una clase de prueba para UserManagerTest y se pretende que
    ///contenga todas las pruebas unitarias UserManagerTest.
    ///</summary>
    [TestClass()]
    public class UserManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Obtiene o establece el contexto de la prueba que proporciona
        ///la información y funcionalidad para la ejecución de pruebas actual.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Atributos de prueba adicionales
        // 
        //Puede utilizar los siguientes atributos adicionales mientras escribe sus pruebas:
        //
        //Use ClassInitialize para ejecutar código antes de ejecutar la primera prueba en la clase 
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup para ejecutar código después de haber ejecutado todas las pruebas en una clase
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize para ejecutar código antes de ejecutar cada prueba
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup para ejecutar código después de que se hayan ejecutado todas las pruebas
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Una prueba de CreateUserIfNotExists
        ///</summary>
        [TestMethod()]
        public void CreateUserIfNotExists_Does_Nothing_On_Existing_User()
        {
            // Arrange

            var userRespositoryMock = new Mock<IUserRepository>();
            var userName = "userName";
            var existingUser = new User() {
                Id = 1,
                UserName = userName
            };

            userRespositoryMock.Setup(rep => rep.GetUserByUserName(userName)).Returns(existingUser);
            userRespositoryMock.Setup(rep => rep.CreateNewUser(It.IsAny<string>())).Throws(new InvalidOperationException("CreateNewUser should not have been called."));
            UserManager userManager = new UserManager(userRespositoryMock.Object, CreateClock());

            // Act

            userManager.CreateUserIfNotExists(userName);

            // Assert

            //(nothing to do, if the test fails we'll get an InvalidOperationException)
        }


        /// <summary>
        ///Una prueba de CreateUserIfNotExists
        ///</summary>
        [TestMethod()]
        public void CreateUserIfNotExists_Returns_False_On_Existing_User()
        {
            // Arrange

            var userRespositoryMock = new Mock<IUserRepository>();
            var userName = "userName";
            var existingUser = new User()
            {
                Id = 1,
                UserName = userName
            };

            userRespositoryMock.Setup(rep => rep.GetUserByUserName(userName)).Returns(existingUser);
            userRespositoryMock.Setup(rep => rep.CreateNewUser(It.IsAny<string>())).Throws(new InvalidOperationException("CreateNewUser should not have been called."));
            UserManager userManager = new UserManager(userRespositoryMock.Object, CreateClock());

            // Act

            var created = userManager.CreateUserIfNotExists(userName);

            // Assert

            Assert.IsFalse(created);
        }


        [TestMethod()]
        public void CreateUserIfNotExists_Creates_User_If_Not_Exists()
        {
            // Arrange

            var userRespositoryMock = new Mock<IUserRepository>();
            var userName = "userName";
            int userId = 1;
            var userHasBeenCreated = false;
            var user = new User()
            {
                Id = userId,
                UserName = userName
            };

            userRespositoryMock.Setup(rep => rep.GetUserByUserName(userName))
                .Returns(() => userHasBeenCreated ? user : (User)null);
            userRespositoryMock.Setup(rep => rep.CreateNewUser(userName))
                .Callback(() =>
                {
                    userHasBeenCreated = true;
                });
            UserManager userManager = new UserManager(userRespositoryMock.Object, CreateClock());

            // Act

            userManager.CreateUserIfNotExists(userName);

            // Assert

            Assert.IsTrue(userHasBeenCreated);
        }


        [TestMethod()]
        public void CreateUserIfNotExists_Returns_True_If_Not_Exists()
        {
            // Arrange

            var userRespositoryMock = new Mock<IUserRepository>();
            var userName = "userName";
            int userId = 1;
            var userHasBeenCreated = false;
            var user = new User()
            {
                Id = userId,
                UserName = userName
            };

            userRespositoryMock.Setup(rep => rep.GetUserByUserName(userName))
                .Returns(() => userHasBeenCreated ? user : (User)null);
            userRespositoryMock.Setup(rep => rep.CreateNewUser(userName))
                .Callback(() =>
                {
                    userHasBeenCreated = true;
                });
            UserManager userManager = new UserManager(userRespositoryMock.Object, CreateClock());

            // Act

            bool created = userManager.CreateUserIfNotExists(userName);

            // Assert

            Assert.IsTrue(created);
        }


        private IClock CreateClock(Func<DateTime> now = null, Func<DateTime> utcNow = null)
        {
            if(now == null) {
                now = () => DateTime.Now;
            }
            if(utcNow == null) {
                utcNow = () => DateTime.UtcNow;
            }

            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(c => c.Now).Returns(now);
            clockMock.SetupGet(c => c.UtcNow).Returns(utcNow);

            return clockMock.Object;
        }


        [TestMethod()]
        public void CreateUserIfNotExists_Creates_User_With_Default_Name()
        {
            // Arrange

            var userRespositoryMock = new Mock<IUserRepository>();
            var userName = "userName";
            int userId = 1;
            var userHasBeenCreated = false;
            var user = new User();

            userRespositoryMock.Setup(rep => rep.GetUserByUserName(userName))
                .Returns(() => userHasBeenCreated ? user : (User)null);
            userRespositoryMock.Setup(rep => rep.CreateNewUser(userName)).Callback(() =>
            {
                userHasBeenCreated = true;
                user.Id = userId;
                user.UserName = userName;
            }).Returns(userId)
              .Verifiable("CreateNewUser is not called.");
            userRespositoryMock.Setup(rep => rep.UpdateUser(user)).Verifiable("UpdateUser is not called.");
            UserManager userManager = new UserManager(userRespositoryMock.Object, CreateClock());

            // Act

            userManager.CreateUserIfNotExists(userName);

            // Assert

            Assert.AreEqual("user" + userId.ToString(), user.DisplayName);
            userRespositoryMock.VerifyAll();
        }


        [TestMethod()]
        public void CreateUserIfNotExists_Creates_User_With_CurrentDate()
        {
            // Arrange

            var fakeDate = new DateTime(2100, 3, 4, 10, 20, 30);
            var clock = CreateClock(utcNow: () => fakeDate);

            var userRespositoryMock = new Mock<IUserRepository>();
            var userName = "userName";
            int userId = 1;
            var userHasBeenCreated = false;
            var user = new User();

            userRespositoryMock.Setup(rep => rep.GetUserByUserName(userName))
                .Returns(() => userHasBeenCreated ? user : (User)null);
            userRespositoryMock.Setup(rep => rep.CreateNewUser(userName)).Callback(() =>
            {
                userHasBeenCreated = true;
                user.Id = userId;
                user.UserName = userName;
            }).Returns(userId);
            UserManager userManager = new UserManager(userRespositoryMock.Object, clock);

            // Act

            userManager.CreateUserIfNotExists(userName);

            // Assert

            Assert.AreEqual(fakeDate, user.JoinedDate);
        }
    }
}
