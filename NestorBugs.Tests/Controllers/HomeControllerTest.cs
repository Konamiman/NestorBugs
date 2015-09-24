using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NestorBugs.Tests;
using NestorBugs.Tests.Controllers;
using Konamiman.NestorBugs.Web.Controllers;

namespace NestorBugs.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        [Ignore]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Faq() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
