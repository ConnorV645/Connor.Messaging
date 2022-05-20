using System;

namespace Connor.Messaging.Interfaces
{
    public interface IMessageItem
    {
        long DiscussionId { get; set; }
        long MessageId { get; set; }
        long UserId { get; set; }
        string Message { get; set; }
        DateTime Sent { get; set; }
    }
}
