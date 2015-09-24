using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Konamiman.NestorBugs.Web.Models
{
    public class LogOnModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName
        {
            get;
            set;
        }

        [Display(Name = "Remember me?")]
        public bool RememberMe
        {
            get;
            set;
        }

        [HiddenInput]
        public string ReturnUrl
        {
            get;
            set;
        }
    }
}
