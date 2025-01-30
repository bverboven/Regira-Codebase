namespace Regira.Invoicing.ViaAdValvas;

public class PeppolResponseException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public int ServiceStatusCode { get; set; }
    public string? SerializedResponse { get; set; }
}