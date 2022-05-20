namespace Connor.Messaging.Interfaces
{
    public interface ITypingMessage : IHasSourceUser
    {
        long DiscussionId { get; set; }
        bool Typing { get; set; }
    }
}
