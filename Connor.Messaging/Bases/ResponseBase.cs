using Connor.Messaging.Enums;
using Connor.Messaging.Interfaces;
using System;

namespace Connor.Messaging.Bases
{
    public class ResponseBase<R> : IResponse<R> where R : Enum
    {
        public ResponseBase() { }
        public ResponseBase(R type)
        {
            RequestType = type;
        }
        public R RequestType { get; set; }
        public bool IsError { get; set; }
        public ErrorCode ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
