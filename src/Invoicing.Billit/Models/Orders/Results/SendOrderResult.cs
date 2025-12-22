using Regira.Invoicing.Billit.Models.Abstractions;

namespace Regira.Invoicing.Billit.Models.Orders.Results;

public record SendOrderResult : IApiResult
{
    public bool IsSuccess { get; set; }
    public Exception? Error { get; set; }
    public string? ErrorResponse { get; set; }
    public int StatusCode { get; set; }
}