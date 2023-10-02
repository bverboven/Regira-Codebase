using Regira.Office.Mail.SendGrid.Extensions;
using Regira.Office.Mail.Web;
using Regira.Serializing.Abstractions;
using Regira.Serializing.Newtonsoft.Json;

namespace Office.Mail.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class SendGridMessageObjectTests
{
    private readonly ISerializer _serializer;
    public SendGridMessageObjectTests()
    {
        _serializer = new JsonSerializer();
    }

    [TestCase(MailConstants.SIMPLE_INPUT)]
    [TestCase(MailConstants.INPUT_NO_SENDER)]
    [TestCase(MailConstants.INPUT_NO_RECIPIENTS)]
    [TestCase(MailConstants.EXTENDED_INPUT)]
    [TestCase(MailConstants.INPUT_NO_SUBJECT)]
    [TestCase(MailConstants.INPUT_NO_BODY)]
    public void Create_MailMessage(string serializedInput)
    {
        var input = _serializer.Deserialize<MailInput>(serializedInput)!;
        var msg = input.ToMessageObject();
        // added [assembly: InternalsVisibleTo("Mail.Testing")] to internal Sendgrid extensions
        var sgMsg = MessageObjectExtensions.ToMailMessage(msg);

        Assert.AreEqual(msg.From?.Email, sgMsg.From?.Email);
        Assert.AreEqual(msg.From?.DisplayName, sgMsg.From?.Name);

        Assert.AreEqual(msg.ReplyTo?.Email, sgMsg.ReplyTo?.Email);
        Assert.AreEqual(msg.ReplyTo?.DisplayName, sgMsg.ReplyTo?.Name);

        // Cannot test recipients -> no such getter on SendGrid's MailMessage object

        Assert.AreEqual(msg.Subject, sgMsg.Subject);
        if (msg.IsHtml)
        {
            Assert.AreEqual(msg.Body, sgMsg.HtmlContent);
            Assert.IsNull(sgMsg.PlainTextContent);
        }
        else
        {
            Assert.AreEqual(msg.Body, sgMsg.PlainTextContent);
            Assert.IsNull(sgMsg.HtmlContent);
        }
    }
}