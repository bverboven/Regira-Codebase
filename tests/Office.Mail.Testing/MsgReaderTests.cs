using Newtonsoft.Json;
using Regira.IO.Extensions;
using Regira.IO.Models;
using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Models;
using Regira.Office.Mail.MSGReader;

namespace Office.Mail.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class MsgReaderTests
{
    private readonly string _assets;
    private readonly IMessageObject _sampleMessageObject;
    public MsgReaderTests()
    {
        _assets = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "../../../Assets")).FullName;
        var sampleObjectPath = Path.Combine(_assets, "serialized-sample.json");
        _sampleMessageObject = JsonConvert.DeserializeObject<MessageObject>(File.ReadAllText(sampleObjectPath))!;
    }


    [Test]
    public void Parse_MSG()
    {
        var msgPath = Path.Combine(_assets, "sample.msg");
        using var msgFile = new BinaryFileItem(msgPath);
        var parser = new MsgParser();
        var msgObj = parser.Parse(msgFile.GetBytes()!.ToMemoryFile());

        Assert.That(msgObj.From!.Email, Is.EqualTo(_sampleMessageObject.From!.Email));
        Assert.That(string.IsNullOrWhiteSpace(msgObj.From!.DisplayName), Is.True);
        Assert.That(msgObj.Body, Is.EqualTo(_sampleMessageObject.Body));
        Assert.That(msgObj.To.Select(x => x.Email), Is.EquivalentTo(_sampleMessageObject.To.Select(x => x.Email)));
        Assert.That(msgObj.Attachments, Is.EquivalentTo(_sampleMessageObject.Attachments!));
    }
    [Test]
    public void Parse_EML()
    {
        var emlPath = Path.Combine(_assets, "sample.eml");
        using var emlFile = new BinaryFileItem(emlPath);
        var parser = new EmlParser();
        var msgObj = parser.Parse(emlFile.GetBytes()!.ToMemoryFile());

        Assert.That(msgObj.From!.Email, Is.EqualTo(_sampleMessageObject.From!.Email));
        //Assert.That(string.IsNullOrWhiteSpace(msgObj.From!.DisplayName), Is.True);
        Assert.That(msgObj.Body, Is.EqualTo(_sampleMessageObject.Body));
        Assert.That(msgObj.To.Select(x => x.Email), Is.EquivalentTo(_sampleMessageObject.To.Select(x => x.Email)));
        Assert.That(msgObj.Attachments, Is.EquivalentTo(_sampleMessageObject.Attachments!));
    }
}