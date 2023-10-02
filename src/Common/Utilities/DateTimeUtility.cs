namespace Regira.Utilities;

public static class DateTimeUtility
{
    public static DateTime FromUnix(double secondsSince1970)
    {
        // https://stackoverflow.com/questions/249760/how-can-i-convert-a-unix-timestamp-to-datetime-and-vice-versa#answer-250400
        return DateTimeOffset
            .FromUnixTimeSeconds((long)secondsSince1970)
            .LocalDateTime;
    }
}