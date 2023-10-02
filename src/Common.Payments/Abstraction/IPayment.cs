using Regira.Payments.Enums;

namespace Regira.Payments.Abstraction;

public interface IPayment
{
    string? Id { get; set; }
    decimal Amount { get; set; }
    string? Currency { get; set; }
    string? Description { get; set; }
    PaymentStatus? Status { get; set; }
    IDictionary<string, object?>? Metadata { get; set; }
    DateTime? CreatedAt { get; set; }
}