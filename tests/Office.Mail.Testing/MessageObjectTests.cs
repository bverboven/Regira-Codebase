using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Exceptions;
using Regira.Office.Mail.Models;
using Regira.Office.Mail.Web;
using Regira.Serializing.Abstractions;
using Regira.Serializing.Newtonsoft.Json;
using System.Dynamic;
using NUnit.Framework.Legacy;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Office.Mail.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class MessageObjectTests
{
    private readonly ISerializer _serializer;
    public MessageObjectTests()
    {
        _serializer = new JsonSerializer();
    }


    [TestCase(null)]
    [TestCase("")]
    [TestCase("bad_email")]
    [TestCase("test@bad_email")]
    public void Error_Invalid_Email(string? email)
    {
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<EmailFormatException>(() => new MailAddress { Email = email });
    }

    [TestCase(MailConstants.SIMPLE_INPUT)]
    public void Create_Simple_MessageObject(string serializedInput)
    {
        dynamic input = _serializer.Deserialize<ExpandoObject>(serializedInput)!;
        var mailInput = _serializer.Deserialize<MailInput>(serializedInput)!;

        var msg = mailInput.ToMessageObject();
        TestMailAddress((MailAddress)msg.From!.Email, input.from);
        ClassicAssert.IsNull(msg.ReplyTo);
        TestMailAddress((MailAddress)msg.To.First().Email, input.to[0]);
        ClassicAssert.AreEqual(msg.Subject, input.subject);
        ClassicAssert.AreEqual(msg.Body, input.body);
        ClassicAssert.IsTrue(msg.IsHtml);
    }

    [TestCase(MailConstants.INPUT_REPLYTO)]
    public void Create_Simple_MessageObject_With_ReplyTo(string serializedInput)
    {
        dynamic input = _serializer.Deserialize<ExpandoObject>(serializedInput)!;
        var mailInput = _serializer.Deserialize<MailInput>(serializedInput)!;

        var msg = mailInput.ToMessageObject();
        ClassicAssert.IsNotNull(msg.ReplyTo);
        TestMailAddress(msg.ReplyTo, input.replyTo);
    }

    [TestCase(MailConstants.INPUT_NO_SENDER)]
    public void Create_MessageObject_Without_Sender(string serializedInput)
    {
        dynamic input = _serializer.Deserialize<ExpandoObject>(serializedInput)!;
        var mailInput = _serializer.Deserialize<MailInput>(serializedInput)!;

        var msg = mailInput.ToMessageObject();
        ClassicAssert.IsNull(msg.From);
        ClassicAssert.IsFalse(((IDictionary<string, object?>)input).ContainsKey("from"));
        TestMailAddress(msg.From!, null);
    }

    [TestCase(MailConstants.INPUT_NO_RECIPIENTS)]
    public void Create_MessageObject_Without_Recipients(string serializedInput)
    {
        dynamic input = _serializer.Deserialize<ExpandoObject>(serializedInput)!;
        var mailInput = _serializer.Deserialize<MailInput>(serializedInput)!;

        var msg = mailInput.ToMessageObject();
        CollectionAssert.IsEmpty(msg.To);
        ClassicAssert.IsFalse(((IDictionary<string, object?>)input).ContainsKey("to"));
        TestMailAddress(msg.To.FirstOrDefault(), null);
    }

    [TestCase(MailConstants.INPUT_NO_HMTL)]
    public void Create_MessageObject_With_Text_Body(string serializedInput)
    {
        var mailInput = _serializer.Deserialize<MailInput>(serializedInput)!;

        var msg = mailInput.ToMessageObject();
        ClassicAssert.IsFalse(msg.IsHtml);
    }

    [TestCase(MailConstants.EXTENDED_INPUT)]
    public void Create_Extended_MessageObject(string serializedInput)
    {
        dynamic input = _serializer.Deserialize<ExpandoObject>(serializedInput)!;
        var inputDic = (IDictionary<string, object?>)input;
        var mailInput = _serializer.Deserialize<MailInput>(serializedInput)!;

        var msg = mailInput.ToMessageObject();
        TestMailAddress(msg.From, input.from);
        if (inputDic.TryGetValue("replyTo", out object? replyTo))
        {
            TestMailAddress(msg.ReplyTo, replyTo);
        }
        for (var i = 0; i < msg.To.Count; i++)
        {
            TestMailAddress(msg.To.ToArray()[i], input.to[i]);
        }
    }

    [TestCase(MailConstants.SIMPLE_INPUT)]
    [TestCase(MailConstants.INPUT_NO_SENDER)]
    [TestCase(MailConstants.INPUT_NO_RECIPIENTS)]
    [TestCase(MailConstants.EXTENDED_INPUT)]
    [TestCase(MailConstants.INPUT_NO_SUBJECT)]
    [TestCase(MailConstants.INPUT_NO_BODY)]
    public void Create_MessageObject(string serializedInput)
    {
        dynamic input = _serializer.Deserialize<ExpandoObject>(serializedInput)!;
        var inputDic = (IDictionary<string, object?>)input;
        var mailInput = _serializer.Deserialize<MailInput>(serializedInput)!;

        var msg = mailInput.ToMessageObject();

        inputDic.TryGetValue("from", out object? from);
        TestMailAddress(msg.From, from);

        inputDic.TryGetValue("replyTo", out object? replyTo);
        TestMailAddress(msg.ReplyTo, replyTo);

        for (var i = 0; i < msg.To.Count; i++)
        {
            TestMailAddress(msg.To.ToArray()[i], input.to[i]);
        }

        inputDic.TryGetValue("subject", out object? subject);
        ClassicAssert.AreEqual(msg.Subject, subject);

        inputDic.TryGetValue("body", out object? body);
        ClassicAssert.AreEqual(msg.Body, body);

        if (inputDic.ContainsKey("isHtml"))
        {
            ClassicAssert.AreEqual(msg.IsHtml, input.isHtml);
        }
        else
        {
            ClassicAssert.IsTrue(msg.IsHtml);
        }
    }


    public static void TestMailAddress(IMailAddress? address, object? input)
    {
        var inputDic = input as IDictionary<string, object?>;
        if (input == null)
        {
            ClassicAssert.IsNull(address);
            return;
        }

        if (inputDic?.ContainsKey("displayName") ?? false)
        {
            ClassicAssert.AreEqual(address!.DisplayName, inputDic["displayName"]);
        }
        else
        {
            ClassicAssert.IsNull(address!.DisplayName);
        }
        if (inputDic?.ContainsKey("email") ?? false)
        {
            ClassicAssert.AreEqual(address.Email, inputDic["email"]);
        }
        else
        {
            ClassicAssert.AreEqual(address.Email, input);
        }
    }
}