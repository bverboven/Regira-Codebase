using Regira.IO.Abstractions;
using Regira.Office.Mail.Abstractions;

namespace Regira.Office.Clients.Models;

public class MailGunInput
{
    public class MailgunConfig
    {
        public string Api { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string Domain { get; set; } = null!;
    }

    public class MailAddress : IMailAddress
    {
        public string? DisplayName { get; set; }
        public string Email { get; set; } = null!;

        public override string ToString() => !string.IsNullOrEmpty(DisplayName) ? DisplayName + " <" + Email + ">" : Email;
    }

    public class MailGunMessage
    {
        public MailAddress From { get; set; } = null!;
        public ICollection<MailAddress> To { get; set; } = null!;


        public MailAddress? ReplyTo { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool IsHtml { get; set; } = true;

        public ICollection<INamedFile>? Attachments { get; set; }
    }

    public MailgunConfig Config { get; set; } = null!;
    public MailGunMessage Message { get; set; } = null!;
}