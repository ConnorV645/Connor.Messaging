namespace Connor.Messaging.Enums
{
    public enum RequestTypeExample : int
    {
        Unknown = 0,
        HeartBeat = 1,
        Send_Message = 2,
        Update_Seen_Time = 3,
        Typing_Message = 4,
        Stop_Typing_Message = 5,
        Load_More_Messages = 6
    }
}
