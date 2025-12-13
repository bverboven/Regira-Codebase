namespace Regira.Invoicing.Billit.Models.Orders;

public class OrderSearchObject
{
    public OrderTypes? OrderType { get; set; }
    public OrderDirections? OrderDirection { get; set; }
}