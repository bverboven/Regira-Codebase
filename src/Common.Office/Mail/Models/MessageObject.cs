using Regira.IO.Abstractions;
using Regira.Office.Mail.Abstractions;

namespace Regira.Office.Mail.Models;

public class MessageObject : IMessageObject
{
    public IMailAddress? From { get; set; }
    public ICollection<IMailRecipient> To { get; set; } = new List<IMailRecipient>();
    public IMailAddress? ReplyTo { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public ICollection<INamedFile>? Attachments { get; set; }
}