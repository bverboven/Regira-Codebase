using Regira.Invoicing.Billit.Models.Contacts;

namespace Regira.Invoicing.Billit.Models.Orders;

public class OrderInput
{
    public class OrderLineInput
    {
        public decimal Quantity { get; set; }
        public decimal UnitPriceExcl { get; set; } // Use decimal for currency/price fields
        public string? Description { get; set; }
        public string? DescriptionExtended { get; set; }
        public string? Reference { get; set; }
        public decimal VATPercentage { get; set; }
    }

    public OrderTypes OrderType { get; set; } = OrderTypes.Invoice;
    public OrderDirections OrderDirection { get; set; } = OrderDirections.Income;
    public string OrderNumber { get; set; } = null!;

    public DateOnly OrderDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string? PaymentReference { get; set; }

    public CustomerInput Customer { get; set; } = null!;
    public List<OrderLineInput> OrderLines { get; set; } = null!;
}