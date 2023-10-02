using Microsoft.Extensions.Configuration;
using Office.Mail.Testing.Abstractions;
using Regira.Office.Mail.MailGun;

namespace Office.Mail.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class MailGunTests : MailerTestsBase
{
    public MailGunTests()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<MailGunTests>()
            .Build();
        Mailer = new MailGunMailer(new MailgunConfig
        {
            Api = config["Mail:MailGun:Api"]!,
            Key = config["Mail:MailGun:Key"]!,
            Domain = config["Mail:MailGun:Domain"]!
        });
    }
}