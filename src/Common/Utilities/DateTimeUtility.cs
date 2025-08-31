namespace Regira.Utilities;

public static class DateTimeUtility
{
    /// <summary>
    /// Converts a Unix timestamp, represented as the number of seconds since January 1, 1970 (UTC), to a local <see cref="DateTime"/>.
    /// </summary>
    /// <param name="secondsSince1970">The number of seconds elapsed since January 1, 1970 (UTC).</param>
    /// <returns>A <see cref="DateTime"/> object representing the local time equivalent of the provided Unix timestamp.</returns>
    public static DateTime FromUnix(double secondsSince1970)
    {
        // https://stackoverflow.com/questions/249760/how-can-i-convert-a-unix-timestamp-to-datetime-and-vice-versa#answer-250400
        return DateTimeOffset
            .FromUnixTimeSeconds((long)secondsSince1970)
            .LocalDateTime;
    }
}