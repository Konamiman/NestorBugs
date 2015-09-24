using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Misc
{
    public class DisplayTextAttribute : Attribute
    {
        public string Text
        {
            get;
            set;
        }

        public DisplayTextAttribute(string text)
        {
            this.Text = text;
        }
    }
}
