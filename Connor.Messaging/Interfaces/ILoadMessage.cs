namespace Connor.Messaging.Interfaces
{
    public interface ILoadMessage
    {
        long DiscussionId { get; set; }
        long UserId { get; set; }
        int Skip { get; set; }
    }
}
