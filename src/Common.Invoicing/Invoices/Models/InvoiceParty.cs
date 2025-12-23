using Regira.Invoicing.Invoices.Models.Abstractions;

namespace Regira.Invoicing.Invoices.Models;

public class InvoiceParty : IInvoiceParty
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;

    public string? Email { get; set; }
    public string? Phone { get; set; }
}