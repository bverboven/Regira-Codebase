using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.Mail.Exceptions;
using Regira.Office.Mail.Extensions;
using Regira.Office.Mail.Models;

namespace Regira.Office.Mail.Abstractions;

public abstract class MailerBase : IMailer
{
    public virtual Task<IMailResponse> Send(IMailAddress sender, IEnumerable<IMailRecipient> recipients, string? subject, string? message, bool isHtml = true, IEnumerable<INamedFile>? attachments = null)
    {
        return Send(new MessageObject
        {
            From = sender.ToMailAddress(),
            To = recipients.ToMailRecipients().ToArray(),
            Subject = subject,
            Body = message,
            IsHtml = isHtml,
            Attachments = attachments?.ToFiles().ToList()
        });
    }
    public virtual Task<IMailResponse> Send(IMessageObject message)
    {
        // Make sure attachments have bytes or a stream 
        if (message.Attachments is { Count: > 0 })
        {
            var namelessFiles = message.Attachments.Where(x => string.IsNullOrWhiteSpace(x.FileName)).ToArray();
            if (namelessFiles.Any())
            {
                throw new MailException($"Attachments ({string.Join(", ", namelessFiles.Select(x => x.FileName))}) have no {nameof(INamedFile.FileName)}");
            }
            var emptyFiles = message.Attachments.Where(x => !x.HasContent()).ToArray();
            if (emptyFiles.Length > 0)
            {
                throw new MailException($"Attachments ({string.Join(", ", emptyFiles.Select(x => x.FileName))}) have no content");
            }
        }

        return OnSend(message);
    }

    protected abstract Task<IMailResponse> OnSend(IMessageObject message);
}