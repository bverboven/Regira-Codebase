namespace Regira.Office.Mail.Exceptions;

public class EmailFormatException : FormatException
{
    public string? EmailInput { get; set; }
    public EmailFormatException(string? input, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        EmailInput = input;
    }
}