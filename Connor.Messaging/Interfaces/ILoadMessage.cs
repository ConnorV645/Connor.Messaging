﻿namespace Connor.Messaging.Interfaces
{
    public interface ILoadMessage : IHasSourceUser
    {
        long DiscussionId { get; set; }
        int Skip { get; set; }
    }
}
