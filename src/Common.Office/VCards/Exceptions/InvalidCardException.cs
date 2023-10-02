namespace Regira.Office.VCards.Exceptions;

public class InvalidCardException : Exception
{
    public string? CardContent { get; }

    public InvalidCardException(string message, Exception? innerException = null)
        : this(message, null, innerException)
    {

    }
    public InvalidCardException(string message, string? cardContent, Exception? innerException = null)
        : base(message, innerException)
    {
        CardContent = cardContent;
    }
}