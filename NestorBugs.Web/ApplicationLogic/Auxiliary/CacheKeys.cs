using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.Auxiliary
{
    public static class CacheKeys
    {
        public static readonly string UserDataChangedBase = "UserData_Changed_{0}";
        public static readonly string UserViewModelBase = "UserViewModel_{0}";
        public static readonly string BugSummaryBase = "BugSummary_{0}";
        public static readonly string BugDataChangedBase = "BugData_Changed_{0}";
        public static readonly string BugDetailsBase = "BugDetails_{0}";
    }
}