# Regira Office.Mail

Regira Office.Mail provides a **unified abstraction** for sending email through multiple providers. All implementations share the same `IMailer` interface, making mail backends interchangeable in consuming code.

## Projects

| Project | Package | Backend |
|---------|---------|---------|
| `Common.Office` | *(transitive)* | Shared abstractions, models, and `DummyMailer` |
| `Mail.SendGrid` | `Regira.Mail.SendGrid` | SendGrid API |
| `Mail.MailGun` | `Regira.Mail.MailGun` | Mailgun REST API |

## Installation

```xml
<!-- SendGrid -->
<PackageReference Include="Regira.Mail.SendGrid" Version="5.*" />

<!-- Mailgun -->
<PackageReference Include="Regira.Mail.MailGun" Version="5.*" />
```

## Quick Start

```csharp
// Register (pick one)
services.AddSendGrid(cfg => cfg.Key = configuration["Mail:SendGrid:Key"]!);
// or
services.AddMailGun(cfg =>
{
    cfg.Api    = configuration["Mail:MailGun:Api"]!;
    cfg.Key    = configuration["Mail:MailGun:Key"]!;
    cfg.Domain = configuration["Mail:MailGun:Domain"]!;
});

// Use
await mailer.Send(
    sender:     "no-reply@example.com",
    recipients: ["alice@example.com"],
    subject:    "Hello",
    message:    "<p>Hi!</p>"
);
```

## IMailer

Both backends implement this interface.

```csharp
// Parameter-based overload
Task<IMailResponse> Send(
    IMailAddress             sender,
    IEnumerable<IMailRecipient> recipients,
    string?                  subject,
    string?                  message,
    bool                     isHtml      = true,
    IEnumerable<INamedFile>? attachments = null);

// Full message overload
Task<IMailResponse> Send(IMessageObject message);
```

### IMailResponse

| Property | Type | Description |
|----------|------|-------------|
| `Success` | `bool` | `true` when the provider accepted the message |
| `Status` | `string?` | HTTP status code or provider status text |
| `Content` | `string?` | Raw response body |
| `Exception` | `Exception?` | Set when sending fails |

## Core Models

### IMessageObject / MessageObject

Represents a complete outgoing email.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `From` | `IMailAddress?` | `null` | Sender address |
| `To` | `ICollection<IMailRecipient>` | `[]` | Recipients (To / Cc / Bcc) |
| `ReplyTo` | `IMailAddress?` | `null` | Reply-To address |
| `Subject` | `string?` | `null` | Email subject |
| `Body` | `string?` | `null` | Message body |
| `IsHtml` | `bool` | `true` | HTML vs plain text |
| `Attachments` | `ICollection<INamedFile>?` | `null` | File attachments |

### IMailAddress / MailAddress

| Property | Type | Description |
|----------|------|-------------|
| `Email` | `string` | Email address — validated on assignment |
| `DisplayName` | `string?` | Optional display name |

`MailAddress` supports implicit conversion from `string`:

```csharp
MailAddress addr  = "alice@example.com";
MailAddress named = new() { Email = "alice@example.com", DisplayName = "Alice" };
```

`ToString()` returns `"Alice <alice@example.com>"` when `DisplayName` is set, or just the email.

### IMailRecipient / MailRecipient

Extends `IMailAddress` with a recipient type.

```csharp
public enum RecipientTypes { To, Cc, Bcc }
```

```csharp
MailRecipient to  = "alice@example.com";   // implicit — defaults to RecipientTypes.To
var cc = new MailRecipient { Email = "bob@example.com",   RecipientType = RecipientTypes.Cc };
var bcc = new MailRecipient { Email = "carol@example.com", RecipientType = RecipientTypes.Bcc };
```

## Configuration

### SendGridConfig

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `string` | SendGrid API key |

### MailgunConfig

| Property | Type | Description |
|----------|------|-------------|
| `Api` | `string` | Mailgun API endpoint (e.g. `https://api.mailgun.net/v3`) |
| `Key` | `string` | Mailgun API key |
| `Domain` | `string` | Sending domain |

## DI Registration

```csharp
// SendGrid
services.AddSendGrid(cfg => cfg.Key = "SG.xxx");

// Mailgun
services.AddMailGun(cfg =>
{
    cfg.Api    = "https://api.mailgun.net/v3";
    cfg.Key    = "key-xxx";
    cfg.Domain = "mail.example.com";
});
```

Both extension methods register `IMailer` as a singleton.

## Exceptions

### MailException

Thrown when the provider returns a non-success response.

| Property | Type | Description |
|----------|------|-------------|
| `MessageObject` | `IMessageObject?` | The message that failed to send |
| `ResponseContent` | `string?` | Raw provider response body |

### EmailFormatException

Thrown when an invalid email address is assigned to `MailAddress.Email`.

| Property | Type | Description |
|----------|------|-------------|
| `EmailInput` | `string?` | The invalid value that was provided |

## Testing — DummyMailer

`DummyMailer` implements `IMailer` and does nothing. Register it in tests to suppress actual sending:

```csharp
services.AddSingleton<IMailer, DummyMailer>();
```

## Web DTOs — MailInput

`Mail.Web` ships `MailInput` for accepting email requests over HTTP. `MailInputExtensions.ToMessageObject()` converts it to a domain `IMessageObject`.

```csharp
[HttpPost]
public async Task<IActionResult> Send([FromBody] MailInput input, IMailer mailer)
{
    var message = input.ToMessageObject();
    var result  = await mailer.Send(message);
    return result.Success ? Ok() : StatusCode(502);
}
```

### MailInput structure

| Property | Type | Validation | Description |
|----------|------|------------|-------------|
| `From` | `Address?` | — | Optional sender override |
| `To` | `ICollection<Recipient>` | `[Required]` | Recipients |
| `ReplyTo` | `Address?` | — | Reply-To address |
| `Subject` | `string?` | `[Required]` | Email subject |
| `Body` | `string?` | — | HTML or plain text body |
| `IsHtml` | `bool` | — | Defaults to `true` |
| `Attachments` | `ICollection<Attachment>?` | — | File attachments |

`Address` and `Recipient` both support implicit conversion from a plain email string.

## ASP.NET Identity Integration

`IdentityMailer` bridges `IMailer` to the ASP.NET Identity `IEmailSender` interface.

```csharp
services.AddSingleton<IEmailSender>(provider =>
    new IdentityMailer(
        provider.GetRequiredService<IMailer>(),
        new IdentityMailerOptions { Sender = "no-reply@example.com" }
    ));
```

## Overview

1. **[Index](README.md)** — Overview, interface, models, and configuration reference
1. [Examples](examples.md) — Simple send, attachments & multiple recipients, backend swap, Identity integration
