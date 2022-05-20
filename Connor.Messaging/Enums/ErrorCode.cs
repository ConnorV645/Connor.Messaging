namespace Connor.Messaging.Enums
{
    public enum ErrorCode : short
    {
        NoError = 0,

        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        Unknown = 404,
        ServerError = 500
    }
}
