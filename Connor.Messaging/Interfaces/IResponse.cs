using Connor.Messaging.Enums;
using System;

namespace Connor.Messaging.Interfaces
{
    public interface IResponse<R> where R : Enum
    {
        R RequestType { get; set; }
        bool IsError { get; set; }
        ErrorCode ErrorCode { get; set; }
        string ErrorMessage { get; set; }
    }
}
