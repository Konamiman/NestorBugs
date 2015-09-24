using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MarkdownSharp;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.HtmlManipulation
{
    class MarkdownToHtmlConverter : IMarkdownToHtmlConverter
    {
        private readonly Markdown markdownConverter;

        public MarkdownToHtmlConverter()
        {
            MarkdownOptions options = new MarkdownOptions()
            {
                AutoHyperlink = true,
                AutoNewLines = true,
                EmptyElementSuffix = "/>",
                EncodeProblemUrlCharacters = true,
                LinkEmails = true,
                StrictBoldItalic = false
            };

            markdownConverter = new Markdown(options);
        }

        public string ConvertMarkdownToHtml(string markdownSharpText)
        {
            var unsanitizedHtml = markdownConverter.Transform(markdownSharpText);
            var sanitizedHtml = unsanitizedHtml.ToSafeHtml();
            var balancedHtml = HtmlBalancer.BalanceTags(sanitizedHtml);
            return balancedHtml;
        }
    }
}