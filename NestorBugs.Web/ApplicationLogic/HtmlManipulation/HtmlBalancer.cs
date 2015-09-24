using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Konamiman.NestorBugs.Web.ApplicationLogic.HtmlManipulation
{
    // CODE FROM: http://refactormycode.com/codes/360-balance-html-tags
    static class HtmlBalancer
    {
        private static Regex _namedtags = new Regex
            (@"</?(?<tagname>\w+)[^>]*(\s|$|>)",
            RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>
        /// attempt to balance HTML tags in the html string
        /// by removing any unmatched opening or closing tags
        /// IMPORTANT: we *assume* HTML has *already* been 
        /// sanitized and is safe/sane before balancing!
        /// 
        /// CODESNIPPET: A8591DBA-D1D3-11DE-947C-BA5556D89593
        /// </summary>
        public static string BalanceTags(string html)
        {
            if(String.IsNullOrEmpty(html))
                return html;

            // convert everything to lower case; this makes
            // our case insensitive comparisons easier
            MatchCollection tags = _namedtags.Matches(html.ToLowerInvariant());

            // no HTML tags present? nothing to do; exit now
            int tagcount = tags.Count;
            if(tagcount == 0)
                return html;

            string tagname;
            string tag;
            const string ignoredtags = "<p><img><br><li><hr>";
            int match;
            var tagpaired = new bool[tagcount];
            var tagremove = new bool[tagcount];

            // loop through matched tags in forward order
            for(int ctag = 0; ctag < tagcount; ctag++) {
                tagname = tags[ctag].Groups["tagname"].Value;

                // skip any already paired tags
                // and skip tags in our ignore list; assume they're self-closed
                if(tagpaired[ctag] || ignoredtags.Contains("<" + tagname + ">"))
                    continue;

                tag = tags[ctag].Value;
                match = -1;

                if(tag.StartsWith("</")) {
                    // this is a closing tag
                    // search backwards (previous tags), look for opening tags
                    for(int ptag = ctag - 1; ptag >= 0; ptag--) {
                        string prevtag = tags[ptag].Value;
                        if(!tagpaired[ptag] && prevtag.Equals("<" + tagname, StringComparison.InvariantCulture)) {
                            // minor optimization; we do a simple possibly incorrect match above
                            // the start tag must be <tag> or <tag{space} to match
                            if(prevtag.StartsWith("<" + tagname + ">") || prevtag.StartsWith("<" + tagname + " ")) {
                                match = ptag;
                                break;
                            }
                        }
                    }
                }
                else {
                    // this is an opening tag
                    // search forwards (next tags), look for closing tags
                    for(int ntag = ctag + 1; ntag < tagcount; ntag++) {
                        if(!tagpaired[ntag] && tags[ntag].Value.Equals("</" + tagname + ">", StringComparison.InvariantCulture)) {
                            match = ntag;
                            break;
                        }
                    }
                }

                // we tried, regardless, if we got this far
                tagpaired[ctag] = true;
                if(match == -1)
                    tagremove[ctag] = true; // mark for removal
                else
                    tagpaired[match] = true; // mark paired
            }

            // loop through tags again, this time in reverse order
            // so we can safely delete all orphaned tags from the string
            for(int ctag = tagcount - 1; ctag >= 0; ctag--) {
                if(tagremove[ctag]) {
                    html = html.Remove(tags[ctag].Index, tags[ctag].Length);
                    System.Diagnostics.Debug.WriteLine("unbalanced tag removed: " + tags[ctag]);
                }
            }

            return html;
        }
    }
}