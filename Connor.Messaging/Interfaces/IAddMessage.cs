using System;

namespace Connor.Messaging.Interfaces
{
    public interface IAddMessage : IHasSourceUser
    {
        DateTime Timestamp { get; set; }
        string Message { get; set; }
        long DiscussionId { get; set; }
    }
}