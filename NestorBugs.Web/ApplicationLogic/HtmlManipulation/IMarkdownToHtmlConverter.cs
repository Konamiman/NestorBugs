using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Konamiman.NestorBugs.CrossCutting.DependencyInjection;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.HtmlManipulation
{
    [RegisterInDependencyInjector(Singleton = true)]
    public interface IMarkdownToHtmlConverter
    {
        string ConvertMarkdownToHtml(string markdownSharpText);
    }
}