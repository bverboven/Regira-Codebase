namespace Regira.Invoicing.ViaAdValvas;

public class PeppolRequestException : Exception
{
    public string? Url { get; set; }
    public int ServiceStatusCode { get; set; }
    public string? ResponseContent { get; set; }

    public PeppolRequestException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}