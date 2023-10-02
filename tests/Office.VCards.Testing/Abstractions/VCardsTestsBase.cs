using Office.VCards.Testing.Samples;
using Regira.Office.VCards.Abstractions;
using Regira.Office.VCards.Exceptions;
using Regira.Office.VCards.Models;
[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Office.VCards.Testing.Abstractions;

public abstract class VCardsTestsBase
{
    private readonly IVCardService _manager;
    protected VCardsTestsBase(IVCardService manager)
    {
        _manager = manager;
    }

    [Test]
    public virtual void Can_Read_Empty()
    {
        var content = @"BEGIN:VCARD
VERSION:2.1
END:VCARD";
        var item = _manager.Read(content);
        Assert.IsNotNull(item);
    }
    [Test]
    public virtual void Read_Invalid_Expect_InvalidCardException()
    {
        var content = "";
        Assert.Throws<InvalidCardException>(() => _manager.Read(content));
    }

    [Test]
    public void Can_Read_V2_1() => CanRead(ContentSamples.VCards[VCardVersion.V2_1], VCardVersion.V2_1);
    [Test]
    public virtual void Can_Read_V3_0() => CanRead(ContentSamples.VCards[VCardVersion.V3_0], VCardVersion.V3_0);
    [Test]
    public virtual void Can_Read_V4_0() => CanRead(ContentSamples.VCards[VCardVersion.V3_0], VCardVersion.V3_0);


    [Test]
    public virtual void Write_Empty()
    {
        var content = _manager.Write(new VCard());
        Assert.IsNotEmpty(content);
    }
    [Test]
    public virtual void Can_Write_V2_1() => CanWrite(CardSamples.BramCard, VCardVersion.V2_1);
    [Test]
    public virtual void Can_Write_V3_0() => CanWrite(CardSamples.BramCard, VCardVersion.V3_0);
    [Test]
    public virtual void Can_Write_V4_0() => CanWrite(CardSamples.BramCard, VCardVersion.V4_0);

    public virtual void CanRead(string content, VCardVersion version)
    {
        var item = _manager.Read(content);
        Assert.IsNotNull(item);
        Assert.AreEqual("Gump", item.Name!.SurName);
        Assert.AreEqual("Forrest", item.Name.GivenName);
        Assert.AreEqual("Mr.", item.Name.Prefix);
        Assert.AreEqual("Forrest Gump", item.FormattedName);
        switch (version)
        {
            case VCardVersion.V2_1:
            case VCardVersion.V3_0:
                Assert.AreEqual("+1-111-555-1212", item.Tels!.First().Uri);
                Assert.AreEqual("+1-404-555-1212", item.Tels!.Last().Uri);
                break;
            case VCardVersion.V4_0:
                Assert.AreEqual("tel:+1-111-555-1212", item.Tels!.First().Uri);
                Assert.AreEqual("tel:+1-404-555-1212", item.Tels!.Last().Uri);
                break;
        }
        Assert.IsTrue((VCardTelType.Cell | VCardTelType.Work) == item.Tels!.First().Type);
        Assert.IsTrue((VCardTelType.Voice | VCardTelType.Home) == item.Tels!.Last().Type);
        Assert.AreEqual("forrestgump@example.com", item.Emails!.First().Text);

        // ReSharper disable PossibleInvalidOperationException
        Assert.AreEqual(VCardPropertyType.Home, item.Emails!.First().Type!.Value);
        Assert.AreEqual(VCardPropertyType.Work, item.Emails!.Last().Type!.Value);
        // ReSharper restore PossibleInvalidOperationException

        Assert.AreEqual("100 Waters Edge", item.Addresses!.First().StreetAndNumber);
        Assert.AreEqual("42 Plantation St.", item.Addresses!.Last().StreetAndNumber);
        Assert.AreEqual("Baytown", item.Addresses!.First().Locality);
        Assert.AreEqual("Baytown", item.Addresses!.Last().Locality);

        Assert.AreEqual("Bubba Gump Shrimp Co.", item.Organization!.Name);
    }
    public virtual void CanWrite(VCard vCard, VCardVersion version)
    {
        var content = _manager.Write(vCard, version);
        Assert.IsTrue(!string.IsNullOrWhiteSpace(content));

        var lines = content.Trim().Split(Environment.NewLine);
        StringAssert.StartsWith("BEGIN:VCARD", lines.First());
        StringAssert.EndsWith("END:VCARD", lines.Last());

        var nameLine = lines.First(l => l.StartsWith("N:"));
        StringAssert.Contains(vCard.Name!.SurName, nameLine);
        StringAssert.Contains(vCard.Name!.GivenName, nameLine);
        StringAssert.Contains(vCard.Name!.Prefix, nameLine);

        var telLines = lines.Where(l => l.StartsWith("TEL")).ToArray();
        StringAssert.Contains(vCard.Tels!.First().Uri!.ToUpper(), telLines.First().ToUpper());
        switch (version)
        {
            case VCardVersion.V2_1:
                StringAssert.Contains("WORK;VOICE", telLines.Last().ToUpper());
                break;
            case VCardVersion.V3_0:
                StringAssert.Contains("WORK,VOICE", telLines.Last().ToUpper());
                break;
            case VCardVersion.V4_0:
                StringAssert.Contains("WORK,VOICE,TEXT", telLines.Last().ToUpper());
                break;
        }
        if (version == VCardVersion.V4_0)
        {
            var emailLines = lines.Where(l => l.StartsWith("EMAIL")).ToArray();
            StringAssert.Contains("HOME", emailLines.Last().ToUpper());
            StringAssert.Contains("WORK", emailLines.Last().ToUpper());
        }

        var addressLines = lines.Where(l => l.StartsWith("ADR")).ToArray();
        StringAssert.Contains("Albertlei 12", addressLines.First());
        switch (version)
        {
            case VCardVersion.V2_1:
                StringAssert.Contains(";HOME", addressLines.First().ToUpper());
                break;
            case VCardVersion.V3_0:
            case VCardVersion.V4_0:
                StringAssert.Contains(";TYPE=HOME", addressLines.First().ToUpper());
                break;
        }

        var orgLines = lines.Where(l => l.StartsWith("ORG"));
        StringAssert.Contains("Regira bv", orgLines.First());
    }
}