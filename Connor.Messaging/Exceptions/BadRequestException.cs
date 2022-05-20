using System;

namespace Connor.Messaging.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string ex) : base(ex)
        {

        }
    }
}
