using Regira.Office.Mail.Abstractions;

namespace Regira.Office.Mail.Exceptions;

public class MailException : Exception
{
    public IMessageObject? MessageObject { get; set; }
    public string? ResponseContent { get; set; }

    public MailException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}