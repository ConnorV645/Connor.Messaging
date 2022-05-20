using System;

namespace Connor.Messaging.Interfaces
{
    public interface IAddMessage
    {
        DateTime Timestamp { get; set; }
        string Message { get; set; }
        long DiscussionId { get; set; }
        long UserId { get; set; }
    }
}