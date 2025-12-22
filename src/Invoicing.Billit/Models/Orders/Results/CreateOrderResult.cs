using Regira.Invoicing.Billit.Models.Abstractions;

namespace Regira.Invoicing.Billit.Models.Orders.Results;

public record CreateOrderResult : IApiResult
{
    public bool IsSuccess { get; set; }
    public string OrderId { get; set; } = null!;
    public Exception? Error { get; set; }
    public string? ErrorResponse { get; set; }
    public int StatusCode { get; set; }
}