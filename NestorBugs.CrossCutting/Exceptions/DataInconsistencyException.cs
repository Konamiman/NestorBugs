using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Exceptions
{
    public class DataInconsistencyException : ApplicationException
    {
        public DataInconsistencyException(string message) : base(message)
        {
        }

        public DataInconsistencyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
