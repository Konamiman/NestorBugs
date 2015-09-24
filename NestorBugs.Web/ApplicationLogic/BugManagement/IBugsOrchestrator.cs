using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.Web.Models;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;
using Konamiman.NestorBugs.Data.Entities;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement
{
    /// <summary>
    /// This class retrieves and stores BugSummaryViewModel and BugDetailsViewModel objects, orchestrating the
    /// cache and database access as necessary.
    /// </summary>
    [RegisterInDependencyInjector(typeof(BugsOrchestrator))]
    public interface IBugsOrchestrator
    {
        IEnumerable<BugSummaryViewModel> GetBugSummaries(int page, int pageSize, BugSortType sortType, string creatorUserName = null);

        BugDetailsViewModel GetBugByUrlTitle(string bugUrlTitle);

        BugDetailsViewModel GetNewBug();
    }
}