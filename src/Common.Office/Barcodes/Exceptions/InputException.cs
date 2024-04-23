namespace Regira.Office.Barcodes.Exceptions;

public class InputException : Exception
{
    public InputException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}