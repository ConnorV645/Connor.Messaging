namespace Connor.Messaging.Interfaces
{
    public interface IReadMessage : IHasSourceUser
    {
        long DiscussionId { get; set; }
        long MessageId { get; set; }
    }
}
