using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Konamiman.NestorBugs.Web.Models
{
    public class UserViewModel
    {
        public string DisplayName
        {
            get;
            set;
        }

        public string ImageUrlSmall
        {
            get;
            set;
        }

        public string ImageUrlBig
        {
            get;
            set;
        }
    }
}
