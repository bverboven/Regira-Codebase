namespace Regira.Invoicing.ViaAdValvas;

public class PeppolRequestException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public string? Url { get; set; }
    public int ServiceStatusCode { get; set; }
    public string? ResponseContent { get; set; }
}