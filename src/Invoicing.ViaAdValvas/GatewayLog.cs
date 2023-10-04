namespace Regira.Invoicing.ViaAdValvas;

internal class GatewayLog
{
    public string? DateTimeStamp { get; set; }
    public GatewayLogType LogType { get; set; }
    public string? Text { get; set; }
}