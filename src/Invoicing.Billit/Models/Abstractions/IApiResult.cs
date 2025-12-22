namespace Regira.Invoicing.Billit.Models.Abstractions;

public interface IApiResult
{
    bool IsSuccess { get; set; }
    Exception? Error { get; set; }
    string? ErrorResponse { get; set; }
    int StatusCode { get; set; }
}