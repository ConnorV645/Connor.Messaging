using System;

namespace Connor.Messaging.Interfaces
{
    public interface IMessageItem : IHasSourceUser
    {
        long DiscussionId { get; set; }
        long MessageId { get; set; }
        string Message { get; set; }
        DateTime Sent { get; set; }
    }
}
