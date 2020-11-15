using System;

namespace Charm.Core.Infrastructure.Exceptions
{
    public static class DatabaseExceptions
    {
        private const string NotFoundExceptionMessage = "Entity with provided id not found";
        private const string NotSingleExceptionMessage = "Single entity expected, more than one found";

        public static Exception NotFound(string message = NotFoundExceptionMessage) => new Exception(message);

        public static Exception NotSingle(string message = NotSingleExceptionMessage) =>
            new Exception(message);
    }
}