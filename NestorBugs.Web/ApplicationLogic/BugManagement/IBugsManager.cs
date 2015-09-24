using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.Web.Models;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;
using Konamiman.NestorBugs.Data.Entities;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement
{
    /// <summary>
    /// Represents a class for performing bug related operations.
    /// This class accesses the database as necessary (via repositories),
    /// but does NOT interact with cache.
    /// </summary>
    [RegisterInDependencyInjector]
    public interface IBugsManager
    {
        /// <summary>
        /// Gets the total count of bugs in the database.
        /// </summary>
        /// <returns></returns>
        int GetBugCount();

        /// <summary>
        /// Gets a list of bug ids.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortType"></param>
        /// <param name="creatorUserName">User name of bug creator. If null, does not filter by bug creator.</param>
        /// <returns></returns>
        IEnumerable<int> GetBugIds(int page, int pageSize, BugSortType sortType, string creatorUserName = null);

        /// <summary>
        /// Get the number of bugs registered in the database.
        /// </summary>
        /// <param name="creatorUserName">If not null, counts only the bugs that have been created
        /// by the user with the specified user name.</param>
        /// <returns></returns>
        int GetTotalBugCount(string creatorUserName = null);

        /// <summary>
        /// Gets summary information for a bug.
        /// </summary>
        /// <param name="bugId"></param>
        /// <returns></returns>
        BugSummaryViewModel GetBugSummary(int bugId);

        /// <summary>
        /// Gets the URL-friendly title for a bug based on its id,
        /// or null if there is no bug with the specified id.
        /// </summary>
        /// <param name="bugId"></param>
        /// <returns></returns>
        string GetBugUrlTitle(int bugId);

        /// <summary>
        /// Gets a full bug from its URL-friendly title,
        /// or null if there is no bug with the specified title.
        /// </summary>
        /// <param name="bugId"></param>
        /// <returns></returns>
        Bug GetBugByUrlTitle(string bugUrlTitle);

        /// <summary>
        /// Gets a full bug from its numeric id
        /// </summary>
        /// <param name="bugId"></param>
        /// <returns></returns>
        Bug GetBugById(int id);

        /// <summary>
        /// Gets a BugDetailsViewModel composed from a Bug object.
        /// Fields that depend on the current user are not filled.
        /// </summary>
        /// <param name="bugDetails"></param>
        /// <returns></returns>
        BugDetailsViewModel CreateBugDetailsViewModel(Bug bugDetails);

        /// <summary>
        /// Fills the fields of a BugDetailsViewModel object that depend on the current user.
        /// </summary>
        /// <param name="bugDetailsViewModel"></param>
        void SetCurrentUserDependentProperties(BugDetailsViewModel bugDetailsViewModel);

        /// <summary>
        /// Retrieves all the registered applications
        /// </summary>
        /// <returns></returns>
        IEnumerable<Application> GetApplications();

        /// <summary>
        /// Telles whether a given user has already submitted bugs to the system or not.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        bool UserHasSubmittedBugs(string userName);

        /// <summary>
        /// Creates a new bug or updates an existing bug.
        /// </summary>
        /// <param name="bug"></param>
        void CreateOrUpdateBug(Bug bug);


        //TODO: Document these methods!

        bool UserHasVotedBug(int bugId, int userId);

        void CastBugVote(BugVote bugVote);

        void RemoveBugVote(int bugId, int userId);

        int GetBugVoteCount(int bugId);

        string CreateBugEditToken(int bugId, int userId);

        string RetrieveBugEditToken(int bugId, int userId);

        void DeleteBugEditToken(int bugId, int userId);

        bool ApplicationExists(int applicationId);
    }
}
