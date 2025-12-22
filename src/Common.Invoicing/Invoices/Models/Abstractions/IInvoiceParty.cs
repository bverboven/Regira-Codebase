namespace Regira.Invoicing.Invoices.Models.Abstractions;

public interface IInvoiceParty
{
    string Code { get; set; }
    string Title { get; set; }
}