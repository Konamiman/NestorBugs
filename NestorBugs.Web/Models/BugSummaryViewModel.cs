using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement;

namespace Konamiman.NestorBugs.Web.Models
{
    public class BugSummaryViewModel
    {
        public string Title
        {
            get;
            set;
        }

        public string UrlTitle
        {
            get;
            set;
        }

        public string ApplicationName
        {
            get;
            set;
        }

        public int VoteCount
        {
            get;
            set;
        }

        public int CommentCount
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string UserUrl
        {
            get;
            set;
        }

        public string UserImageUrl
        {
            get;
            set;
        }

        public DateTime CreationDate
        {
            get;
            set;
        }

        public DateTime? LastEditDate
        {
            get;
            set;
        }

        public int Status
        {
            get;
            set;
        }

        public int? CloseReason
        {
            get;
            set;
        }

        public bool IsClosed
        {
            get
            {
                return Status >= (byte)BugStatus.ClosedFixed;
            }
        }

        public bool IsActive
        {
            get
            {
                return Status == (byte)BugStatus.WorkInProgress;
            }
        }
    }
}