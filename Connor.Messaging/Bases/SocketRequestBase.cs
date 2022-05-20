using Connor.Messaging.Enums;
using Newtonsoft.Json.Linq;
using System;

namespace Connor.Messaging.Bases
{
    public class SocketRequestBase<R> where R : Enum
    {
        public R RequestType { get; set; }
        public JToken Data { get; set; }
    }
}
