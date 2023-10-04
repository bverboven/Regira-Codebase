namespace Regira.Invoicing.ViaAdValvas;

public class PeppolResponseException : Exception
{
    public int ServiceStatusCode { get; set; }
    public string? SerializedResponse { get; set; }

    public PeppolResponseException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}