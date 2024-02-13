using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Models;
using System.Text;

namespace Regira.Office.Mail.MSGReader;

public class EmlParser : IMessageParser
{
    public IMessageObject Parse(IMemoryFile emlFile)
    {
        using var emlStream = emlFile.GetStream();
        var eml = new MsgReader.Mime.Message(emlStream);

        var bodyEncoding = eml.HtmlBody?.BodyEncoding ?? eml.TextBody?.BodyEncoding ?? Encoding.UTF8;
        var bodyBytes = eml.HtmlBody?.Body ?? eml.TextBody?.Body;

        return new MessageObject
        {
            From = eml.Headers.From.ToRecipient(),
            To = eml.Headers.To.ToRecipients()
                .Concat(eml.Headers.Cc.ToRecipients(RecipientTypes.Cc))
                .Concat(eml.Headers.Bcc.ToRecipients(RecipientTypes.Bcc))
                .ToList(),
            ReplyTo = eml.Headers.ReplyTo?.ToRecipient(),
            Subject = eml.Headers.Subject,
            Body = bodyBytes != null ? bodyEncoding.GetString(bodyBytes) : null,
            IsHtml = eml.HtmlBody?.Body?.Any() ?? false,
            Attachments = eml.Attachments.ToFiles().ToList()
        };
    }
}