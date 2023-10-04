namespace Regira.Invoicing.ViaAdValvas;

internal class ValidateResponse
{
    public ICollection<GatewayLog>? Logs { get; set; }
    public bool Status { get; set; }
    public string? DocumentIdentifier { get; set; }
}