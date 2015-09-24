using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.Configuration;
using Moq;
using Konamiman.NestorBugs.Data.Entities;

namespace Konamiman.NestorBugs.Data.Tools
{
    public class FakeDataGenerator : IFakeDataGenerator
    {
        private readonly int fakeBugsCount;
        private readonly int fakeUsersCount;
        private readonly int fakeApplicationscount;
        private NestorBugsEntities dbContext = null;
        private User[] users;
        private Application[] applications;
        private readonly Random randomNumberGenerator;
        private readonly Dictionary<User, Mock<User>> userMocks = new Dictionary<User, Mock<User>>();
        private readonly Dictionary<Application, Mock<Application>> applicationMocks = new Dictionary<Application, Mock<Application>>();
        private string connectionString;

        const int MinutesInYear = 365 * 24 * 60;
        const int MinutesInMonth = 30 * 24 * 60;
        const int MinutesInWeek = 7 * 24 * 60;
        const int maxVotesPerBug = 100;
        const int maxCommentsPerBug = 200;

        public FakeDataGenerator(IConfigurationManager configurationManager)
        {
            fakeBugsCount = configurationManager.GetConfigurationValue<int>(ConfigurationKeys.FakeBugsCount);
            fakeUsersCount = configurationManager.GetConfigurationValue<int>(ConfigurationKeys.FakeUsersCount);
            randomNumberGenerator = new Random(DateTime.Now.Millisecond);
            fakeApplicationscount = GetRandomNumber(5, 20);
        }


        private void CreateDataContext()
        {
            if(dbContext != null) {
                dbContext.Dispose();
            }
            dbContext = new NestorBugsEntities(connectionString);
        }


        public void FillWithFakeData(string connectionString)
        {
            this.connectionString = connectionString;

            CreateFakeUsers();
            CreateFakeApplications();
            CreateFakeBugs();
        }


        private void CreateFakeUsers()
        {
            CreateDataContext();

            for(int i = 1; i <= fakeUsersCount; i++) {
                var user = new User();

                user.DisplayName = "User " + i.ToString();
                user.Email = string.Format("user{0}@example.com", i);
                user.Id = i;
                user.JoinedDate = GetRandomDate();
                user.Url = (i % 2 == 0 ? null : string.Format("http://user-{0}.example.com", i));
                user.UserName = "user" + i.ToString();

                dbContext.Users.Add(user);
            }

            dbContext.SaveChanges();

            users = dbContext.Users.ToArray();
        }



        private void CreateFakeApplications()
        {
            CreateDataContext();

            for(int i = 1; i < fakeApplicationscount; i++) {
                var application = new Application();

                application.Id = i;
                application.Name = "Application " + i.ToString();

                dbContext.Applications.Add(application);
            }

            dbContext.SaveChanges();

            applications = dbContext.Applications.ToArray();
        }


        private void CreateFakeBugs()
        {
            CreateDataContext();

            for(int i = 1; i <= fakeBugsCount; i++) {
                var bug = new Bug();

                bug.ApplicationVersion = "v"+i.ToString();
                bug.CreationDate = GetRandomDate();
                bug.Description="**Description** for bug " + i.ToString() + "\r\n*Cool...*";
                bug.Environment = "Environment for bug " + i.ToString();
                bug.Id = i;
                bug.Status = (byte)GetRandomNumber(1, 7);
                bug.Title = "This is the bug " + i.ToString();
                bug.UrlTitle = "this-is-the-bug-" + i.ToString();

                bug.Locked = (i % 7 == 0);
                if(i % 9 == 0) {
                    bug.DuplicateBugId = i - 1;
                }

                if(i % 3 == 0) {
                    bug.LastEditDate = GetRandomDate();
                    bug.LastEditUserId = GetRandomItem(users).Id;
                }

                bug.Application = GetRandomItem(applications);
                bug.ApplicationId = bug.Application.Id;

                bug.UserId = GetRandomItem(users).Id;

                CreateFakeBugVotes(bug);
                CreateFakeBugComments(bug);

                dbContext.Bugs.Add(bug);
            }

            dbContext.SaveChanges();
        }


        private void CreateFakeBugVotes(Bug bug)
        {
            for(int v = 1; v < GetRandomNumber(0, maxVotesPerBug); v++) {
                var vote = new BugVote();
                var user = GetRandomItem(users);
                
                vote.IssueDate = GetRandomDate();

                vote.UserId = user.Id;

                bug.Votes.Add(vote);
            }
        }


        private void CreateFakeBugComments(Bug bug)
        {
            int index = 1;
            for(int c = 1; c < GetRandomNumber(0, maxCommentsPerBug); c++) {
                var user = GetRandomItem(users);

                var comment = new BugComment();

                comment.PostDate = GetRandomDate();
                comment.Text = string.Format("This is comment {0} for bug {1}", index++, bug.Id);
                comment.UserId = user.Id;

                bug.Comments.Add(comment);
            }
        }


        private byte GetRandomEnumValue<T>()
        {
            var values = Enum.GetValues(typeof(T)).Cast<int>();
            return (byte)GetRandomNumber(values.Min(), values.Max());
        }

        private DateTime GetRandomDate()
        {
            return DateTime.Now.AddMinutes(GetRandomNumber(-MinutesInWeek, 0));
        }

        private T GetRandomItem<T>(IEnumerable<T> items)
        {
            return items.Skip(GetRandomNumber(0, items.Count() - 1)).First();
        }

        private int GetRandomNumber(int minimum, int maximum)
        {
            return randomNumberGenerator.Next(minimum, maximum + 1);
        }
    }
}

