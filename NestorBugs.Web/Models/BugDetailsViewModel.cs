using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.Data.Entities;

namespace Konamiman.NestorBugs.Web.Models
{
    public class BugDetailsViewModel
    {
        public Bug Bug
        {
            get;
            set;
        }


        /* Displayed properties */

        public string DuplicateBugTitle
        {
            get;
            set;
        }

        public int VotesCount
        {
            get;
            set;
        }

        public string ApplicationName
        {
            get;
            set;
        }

        public string CreatorDisplayName
        {
            get;
            set;
        }

        public string CreatorUrl
        {
            get;
            set;
        }

        public string CreatorImageUrl
        {
            get;
            set;
        }

        public string LastEditorDisplayName
        {
            get;
            set;
        }

        public string LastEditorUrl
        {
            get;
            set;
        }

        public string LastEditorImageUrl
        {
            get;
            set;
        }

        public string BugDescriptionHtml
        {
            get;
            set;
        }

        public string BugTestingEnvironmentHtml
        {
            get;
            set;
        }

       
        /* Helper properties */

        public bool BugHasEdits
        {
            get
            {
                return Bug.LastEditDate.HasValue;
            }
        }

        public string DuplicateBugUrlTitle
        {
            get;
            set;
        }

        public bool UserIsLoggedIn
        {
            get;
            set;
        }

        public bool CurrentUserHasVotedThisBug
        {
            get;
            set;
        }

        public bool CurrentUserIsBugCreator
        {
            get;
            set;
        }

        public bool CurrentUserIsSiteOwner
        {
            get;
            set;
        }

        public bool CurrentUserCanEditBug
        {
            get
            {
                return CurrentUserIsSiteOwner || (!Bug.Locked && CurrentUserIsBugCreator);
            }
        }

        public bool IsNewBug
        {
            get
            {
                return Bug.Id == 0;
            }
        }

        public string BugIdToken
        {
            get;
            set;
        }
    }
}