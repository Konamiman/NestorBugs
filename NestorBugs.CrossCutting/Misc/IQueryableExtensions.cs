using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.NestorBugs.CrossCutting.Exceptions;

namespace Konamiman.NestorBugs.CrossCutting.Misc
{
    /// <summary>
    /// Provides "safe" versions of Single() and SingleOrDefault() for IEnumerables.
    /// If possible, acts like First() and logs the error. If not, throws the appropriate error
    /// (a DatabaseIncosistencyException instead of an InvalidOperationException).
    /// </summary>
    public static class IQueryableExtensions
    {
        // Must be set manually at application startup time.
        public static IExceptionLogger ExceptionLogger
        {
            get;
            set;
        }


        public static T SafeSingleOrDefault<T>(this IEnumerable<T> query)
        {
            try {
                return query.SingleOrDefault();
            }
            catch(InvalidOperationException) {
                var count = query.Count();
                var message = string.Format(
                    "Found {0} elements of type '{1}' with the given criteria, expected zero or one.",
                    count, typeof(T).Name);

                LogError(message);
                return query.First();
            }
        }


        public static T SafeSingle<T>(this IEnumerable<T> query)
        {
            try {
                return query.Single();
            }
            catch(InvalidOperationException) {
                var count = query.Count();
                var message = string.Format(
                    "Found {0} elements of type '{1}' with the given criteria, expected one.",
                    query.Count(), typeof(T).Name);
                if(count > 0) {
                    LogError(message);
                    return query.First();
                }
                else {
                    throw new DataInconsistencyException(message);
                }
            }
        }


        static void LogError(string message)
        {
            var exception = new DataInconsistencyException(message);
            ExceptionLogger.Log(exception);
        }
    }
}
