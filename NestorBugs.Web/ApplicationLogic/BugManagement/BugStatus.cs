using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.CrossCutting.Misc;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.BugManagement
{
    public enum BugStatus
    {
        Proposed = 1,

        [DisplayText("Work in progress")]
        WorkInProgress = 2,

        Acknowledged = 3,

        [DisplayText("Closed (Fixed)")]
        ClosedFixed = 4,

        [DisplayText("Closed (By Design)")]
        ClosedByDesign = 5,

        [DisplayText("Closed (Can't Reproduce)")]
        ClosedCantReproduce = 6,

        [DisplayText("Closed (Spam)")]
        ClosedSpam = 7
    }
}