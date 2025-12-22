using Regira.Invoicing.Invoices.Models.Abstractions;
using Regira.IO.Models;

namespace Regira.Invoicing.Invoices.Models;

public class Invoice : IInvoice
{
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public InvoiceType InvoiceType { get; set; }
#if NETSTANDARD
    public  DateTime IssueDate { get; set; }
    public  DateTime? DueDate { get; set; }
#else
    public DateOnly IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }
#endif
    public string? RemittanceInfo { get; set; }
    public InvoiceParty? Supplier { get; set; }
    public InvoiceParty Customer { get; set; } = new();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = [];
    public ICollection<BinaryFileItem>? Attachments { get; set; }
}