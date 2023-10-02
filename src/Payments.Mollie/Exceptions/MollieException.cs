using System.Net;

namespace Regira.Payments.Mollie.Exceptions;

public class MollieException : WebException, IPaymentException
{
    public string? MollieResponse { get; set; }

    public MollieException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}