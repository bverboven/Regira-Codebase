namespace Regira.Payments.Pom;

public class PomException : Exception
{
    public string? PomResponse { get; set; }
    public int StatusCode { get; set; }

    public PomException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}