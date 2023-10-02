using Regira.Office.VCards.Models;

namespace Regira.Office.VCards.Abstractions;

public interface IVCardService
{
    VCard Read(string content);
    IEnumerable<VCard> ReadMany(string content);

    string Write(VCard item, VCardVersion version = VCardVersion.V3_0);
    string Write(IEnumerable<VCard> items, VCardVersion version = VCardVersion.V3_0);
}