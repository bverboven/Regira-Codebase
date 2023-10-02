using Regira.IO.Abstractions;

namespace Regira.Office.Mail.Abstractions;

public interface IMailer
{
    Task<IMailResponse> Send(IMailAddress sender, IEnumerable<IMailRecipient> recipients, string? subject, string? message, bool isHtml = true, IEnumerable<INamedFile>? attachments = null);
    Task<IMailResponse> Send(IMessageObject message);
}