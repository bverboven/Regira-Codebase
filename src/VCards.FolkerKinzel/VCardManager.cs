using FolkerKinzel.VCards;
using FolkerKinzel.VCards.Extensions;
using FolkerKinzel.VCards.Models.Enums;
using Regira.IO.Utilities;
using Regira.Office.VCards.Abstractions;
using Regira.Office.VCards.Exceptions;
using static Regira.Office.VCards.FolkerKinzel.Converting.Converter;
using FKvCard = FolkerKinzel.VCards.VCard;
using VCard = Regira.Office.VCards.Models.VCard;

namespace Regira.Office.VCards.FolkerKinzel;

public class VCardManager : IVCardService
{
    public VCard Read(string content)
    {
        return ReadMany(content).First();
    }
    public IEnumerable<VCard> ReadMany(string content)
    {
        var tr = new StringReader(content);
        var items = FKvCard.DeserializeVcf(tr);
        if (!items.Any())
        {
            throw new InvalidCardException("Invalid card", content);
        }
        return items.Select(Convert);
    }

    public string Write(VCard item, VCardVersion version = VCardVersion.V3_0)
    {
        var vCard = Convert(item);
        var stream = new MemoryStream();
        vCard.SerializeVcf(stream, GetVersion(version), VcfOptions.Default, true);
        return FileUtility.GetString(stream)!;
    }
    public string Write(IEnumerable<VCard> items, VCardVersion version = VCardVersion.V3_0)
    {
        var stream = new MemoryStream();
        items.Select(Convert).ToList().SerializeVcf(stream, GetVersion(version), VcfOptions.Default, true);
        return FileUtility.GetString(stream)!;
    }

    VCdVersion GetVersion(VCardVersion version)
    {
        switch (version)
        {
            case VCardVersion.V2_1:
                return VCdVersion.V2_1;
            case VCardVersion.V3_0:
                return VCdVersion.V3_0;
            case VCardVersion.V4_0:
                return VCdVersion.V4_0;
            default:
                return VCdVersion.V3_0;
        }
    }
}