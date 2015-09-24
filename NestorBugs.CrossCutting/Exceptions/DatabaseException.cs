using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konamiman.NestorBugs.CrossCutting.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when accessing the database engine.
    /// </summary>
    public class DatabaseException : ApplicationException
    {
        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
