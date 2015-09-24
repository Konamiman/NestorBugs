using Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Collections.Generic;
using Konamiman.NestorBugs.Data;
using Konamiman.NestorBugs.Data.Entities;
using Moq;
using System.Data.Entity;
using NestorBugs.Tests.Helpers;
using System.Linq;
using Konamiman.NestorBugs.CrossCutting.Configuration;

namespace NestorBugs.Tests.ApplicationLogic.BugManagement
{
    
    
    /// <summary>
    ///Se trata de una clase de prueba para BugsManagerTest y se pretende que
    ///contenga todas las pruebas unitarias BugsManagerTest.
    ///</summary>
    [TestClass()]
    public class BugsManagerTest
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


        [TestMethod]
        public void GetBugCount_Returns_Valid_Value()
        {
            var bugsManager = GetFakeBugsManagerWithData();

            var count = bugsManager.GetBugCount();

            Assert.AreEqual(20, count);
        }

        [TestMethod]
        public void GetBugIds_Returns_Proper_Number_Of_Bugs_On_Full_Page_Request()
        {
            var bugsManager = GetFakeBugsManagerWithData();

            var bugIds = bugsManager.GetBugIds(page: 1, pageSize: 7, sortType: BugSortType.Newest);

            Assert.AreEqual(7, bugIds.Count());
        }

        [TestMethod]
        public void GetBugIds_Returns_Proper_Number_Of_Bugs_On_Partial_Page_Request()
        {
            var bugsManager = GetFakeBugsManagerWithData();

            var bugIds = bugsManager.GetBugIds(page: 2, pageSize: 15, sortType: BugSortType.Newest);

            Assert.AreEqual(5, bugIds.Count());
        }

        [TestMethod]
        public void GetBugIds_Returns_No_Bugs_On_Too_High_Page_Request()
        {
            var bugsManager = GetFakeBugsManagerWithData();

            var bugIds = bugsManager.GetBugIds(page: 3, pageSize: 10, sortType: BugSortType.Newest);

            Assert.AreEqual(0, bugIds.Count());
        }


        [TestMethod]
        public void GetBugIds_Returns_Proper_Ordered_Bugs_On_Newest_Sort_Request()
        {
            var bugsManager = GetFakeBugsManagerWithData();

            var bugIds = bugsManager.GetBugIds(page: 1, pageSize: 20, sortType: BugSortType.Newest);

            var expectedIds = Enumerable.Range(1, 20).Reverse();
            Assert.IsTrue(bugIds.SequenceEqual(expectedIds));
        }

        [TestMethod]
        public void GetBugIds_Returns_Proper_Ordered_Bugs_On_Recent_Sort_Request()
        {
            var bugsManager = GetFakeBugsManagerWithData();

            var bugIds = bugsManager.GetBugIds(page: 1, pageSize: 20, sortType: BugSortType.Recent);

            var expectedIds = (new int[] {
                1, 3, 5, 7, 9, 11, 13, 15, 17, 19,
                2, 4, 6, 8, 10, 12, 14, 16, 18, 20}).Reverse();
            Assert.IsTrue(bugIds.SequenceEqual(expectedIds));
        }

        [TestMethod]
        public void GetBugIds_Returns_Proper_Ordered_Bugs_On_MostVoted_Sort_Request()
        {
            var bugsManager = GetFakeBugsManagerWithData();

            var bugIds = bugsManager.GetBugIds(page: 1, pageSize: 20, sortType: BugSortType.MostVoted);

            var expectedIds = new int[] {
                5, 10, 15, 20,
                4, 9, 14, 19,
                3, 8, 13, 18,
                2, 7, 12, 17,
                1, 6, 11, 16};
            Assert.IsTrue(bugIds.SequenceEqual(expectedIds));
        }

        #region Auxiliary methods

        private BugsManager GetFakeBugsManagerWithData()
        {
            var dbContextMoq = GetFakeBugDbContextWithData();
            return new BugsManager(dbContextMoq.Object);
        }

        private Mock<NestorBugsEntities> GetFakeBugDbContextWithData()
        {
            var fakeDbSet = new FakeDbSet<Bug>();
            for(int i = 1; i <= 20; i++) {
                var bugMock = new Mock<Bug>();
                var votesMock = new Mock<BugVote>();
                bugMock.Setup(b => b.Votes).Returns(new List<BugVote>());
                var commentsMock = new Mock<BugComment>();
                bugMock.Setup(b => b.Comments).Returns(new List<BugComment>());
                var bug = bugMock.Object;

                bug.Id = i;
                bug.Application = new Application()
                {
                    Name = "Application " + i.ToString()
                };
                bug.User = new User()
                {
                    UserName = "user" + i.ToString(),
                    DisplayName = "User " + i.ToString()
                };
                bug.Title = "Bug #" + i.ToString();
                bug.UrlTitle = "bug-" + i.ToString();

                fakeDbSet.Add(bug);
            }

            // Bugs will have a vote count equal to bug # modulo 5:
            // 1, 2, 3, 4, 5, 1, 2, 3...

            int count = 1;
            int id = 1;
            foreach(var bug in fakeDbSet) {
                var votes = Enumerable.Repeat<BugVote>(new BugVote() { Id = id }, count);
                foreach(var vote in votes) {
                    bug.Votes.Add(vote);
                    id++;
                }
                count = (count % 5) + 1;
            }

            // Bugs will have a comment count equal to bug # modulo 10:
            // 1, 2, 3, ..., 9, 10, 1, 2, 3...

            count = 1;
            id = 1;
            foreach(var bug in fakeDbSet) {
                var comments = Enumerable.Repeat<BugComment>(new BugComment() { Id = id }, count);
                foreach(var comment in comments) {
                    bug.Comments.Add(comment);
                    id++;
                }
                count = (count % 10) + 1;
            }

            // Bugs will have a creation date equal to Jan 1 2010
            // plus the number of days indicated by the bug id, so:
            // Jan 1 2010, Jan 2, Jan 3...

            var date = new DateTime(2010, 1, 1);
            foreach(var bug in fakeDbSet) {
                bug.CreationDate = date;
                date = date.AddDays(1);
            }

            // Even bugs will have last edit date equal to creation date;
            // same for odd bugs but one year ahead.

            bool even = true;
            foreach(var bug in fakeDbSet) {
                if(even) {
                    bug.LastEditDate = bug.CreationDate;
                }
                else {
                    bug.LastEditDate = bug.CreationDate.AddYears(1);
                }
                even = !even;
            }

            var dbContextMoq = new Mock<NestorBugsEntities>();
            dbContextMoq.Setup(db => db.Bugs).Returns(fakeDbSet);
            return dbContextMoq;
        }

        #endregion
    }
}
