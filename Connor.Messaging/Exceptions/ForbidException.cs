using System;

namespace Connor.Messaging.Exceptions
{
    public class ForbidException : Exception
    {
        public ForbidException(string ex) : base(ex)
        {

        }
    }
}
