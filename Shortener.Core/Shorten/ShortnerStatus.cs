namespace Shortener.Core.Shorten
{
    public enum ShortnerStatus
    {
        Success,
        InvalidUrl,
        InvalidVanity,
        InvalidTitle,
        InvalidSchedule,
        InvalidRequest,
        Conflict,
        UnknownError
    }
}
