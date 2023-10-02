using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Exceptions;
using Regira.Office.Mail.Models;
using Regira.Office.Mail.SendGrid.Extensions;
using SendGrid;
using System.Net;

namespace Regira.Office.Mail.SendGrid;

public class SendGridMailer : MailerBase
{
    private readonly SendGridClient _client;
    public SendGridMailer(SendGridConfig config)
    {
        _client = new SendGridClient(config.Key);
    }

    protected override async Task<IMailResponse> OnSend(IMessageObject message)
    {
        var mail = message.ToMailMessage();
        var mailerResponse = await _client.SendEmailAsync(mail);

        if (mailerResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new Exception("Not authorized");
        }

        var responseContent = await mailerResponse.Body.ReadAsStringAsync();
        if (!mailerResponse.IsSuccessStatusCode)
        {
            throw new MailException($"Sending message failed. Status: {mailerResponse.StatusCode}")
            {
                MessageObject = message,
                ResponseContent = responseContent
            };
        }

        var response = new MailResponse
        {
            Status = mailerResponse.StatusCode.ToString(),
            Content = responseContent,
            Success = mailerResponse.IsSuccessStatusCode
        };

        return response;
    }
}