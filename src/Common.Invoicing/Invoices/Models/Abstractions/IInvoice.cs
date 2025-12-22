using Regira.IO.Models;

namespace Regira.Invoicing.Invoices.Models.Abstractions;

public interface IInvoice
{
    string Code { get; set; }
    string? Title { get; set; }
    string? Description { get; set; }
    InvoiceType InvoiceType { get; set; }
#if NETSTANDARD
    DateTime IssueDate { get; set; }
    DateTime? DueDate { get; set; }
#else
    DateOnly IssueDate { get; set; }
    DateOnly? DueDate { get; set; }
#endif
    string? RemittanceInfo { get; set; }
    InvoiceParty? Supplier { get; set; }
    InvoiceParty Customer { get; set; }
    ICollection<InvoiceLine> InvoiceLines { get; set; }
    ICollection<BinaryFileItem>? Attachments { get; set; }
}