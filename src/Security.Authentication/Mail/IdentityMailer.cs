using Microsoft.AspNetCore.Identity.UI.Services;
using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Models;

namespace Regira.Security.Authentication.Mail;

public class IdentityMailer : IEmailSender
{
    private readonly IMailer _mailer;
    public IdentityMailer(IMailer mailer)
    {
        _mailer = mailer;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var msg = new MessageObject
        {
            To = { new MailRecipient { Email = email } },
            Subject = subject,
            Body = htmlMessage
        };
        return _mailer.Send(msg);
    }
}