using Microsoft.Extensions.Configuration;
using Office.Mail.Testing.Abstractions;
using Regira.Office.Mail.SendGrid;

namespace Office.Mail.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class SendGridTests : MailerTestsBase
{
    public SendGridTests()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<SendGridTests>()
            .Build();
        Mailer = new SendGridMailer(new SendGridConfig
        {
            Key = config["Mail:SendGrid:Key"]!
        });
    }
}