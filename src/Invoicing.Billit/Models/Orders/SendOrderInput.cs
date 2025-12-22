namespace Regira.Invoicing.Billit.Models.Orders;

public class SendOrderInput
{
    public string TransportType => "Peppol";
    public ICollection<int> OrderIDs { get; set; } = null!;
}