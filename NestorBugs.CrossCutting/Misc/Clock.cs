using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Misc
{
    class Clock : IClock
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
