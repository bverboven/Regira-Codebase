using NUnit.Framework.Legacy;
using Regira.IO.Models;
using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Models;
using System.Reflection;

namespace Office.Mail.Testing.Abstractions;

public abstract class MailerTestsBase
{
    protected IMailer Mailer = null!;
    protected readonly string Assets;
    protected MailerTestsBase()
    {
        // ReSharper disable AssignNullToNotNullAttribute
        Assets = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "../../..", "Assets")).FullName;
        // ReSharper restore AssignNullToNotNullAttribute
    }

    public async virtual Task Send_Without_Attachment()
    {
        var msg = new MessageObject
        {
            From = (MailAddress)"bram@regira.com",
            To = { (MailRecipient)"bramverboven@hotmail.com" },
            Subject = $"Test from {Mailer.GetType().Name}",
            Body = "Testing without attachment..."
        };
        var response = await Mailer.Send(msg);

        Assert.That(response.Success, Is.True);
    }
    public async virtual Task Send_With_Attachment()
    {
        var attachment = new BinaryFileItem
        {
            FileName = "file1.pdf",
            Bytes = await File.ReadAllBytesAsync(Path.Combine(Assets, "file1.pdf"))
        };
        var msg = new MessageObject
        {
            From = (MailAddress)"bram@regira.com",
            To = { (MailRecipient)"bramverboven@hotmail.com" },
            Subject = $"Test from {Mailer.GetType().Name}",
            Body = "Testing with attachment...",
            Attachments = new List<BinaryFileItem> { attachment }
        };
        var response = await Mailer.Send(msg);

        Assert.That(response.Success, Is.True);
    }
}