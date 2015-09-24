using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Misc
{
    public class UrlNormalizer : IUrlNormalizer
    {
        public string GetNormalizedUrl(string url)
        {
            try {
                url = new Uri(url).AbsoluteUri;
            }
            catch(UriFormatException) {
                //If not a valid URL after all, simply use manual normalization
            }

            return NormalizeManually(url);
        }

        private string NormalizeManually(string url)
        {
            var index = url.IndexOf("//");
            if(index == -1) {
                return url.TrimEnd('/');
            }
            else {
                return url.Substring(index + 2).TrimEnd('/');
            }
        }
    }
}
