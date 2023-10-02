using Regira.Office.VCards.Models;

namespace Regira.Office.VCards.Abstractions;

public abstract class VCardManagerBase : IVCardService
{
    public abstract VCard Read(string content);
    public abstract IEnumerable<VCard> ReadMany(string content);

    public abstract string Write(VCard item, VCardVersion version = VCardVersion.V3_0);
    public abstract string Write(IEnumerable<VCard> items, VCardVersion version = VCardVersion.V3_0);
}