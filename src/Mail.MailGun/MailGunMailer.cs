using System.Net;
using Regira.IO.Extensions;
using Regira.IO.Utilities;
using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace Regira.Office.Mail.MailGun;

public class MailGunMailer(MailgunConfig config) : MailerBase
{
    private readonly string _mailgunApi = config.Api;
    private readonly string _mailgunKey = config.Key;
    private readonly string _domain = config.Domain;


    protected override async Task<IMailResponse> OnSend(IMessageObject message)
    {
        var client = new RestClient(_mailgunApi, c => c.Authenticator = new HttpBasicAuthenticator("api", _mailgunKey));

        var request = new RestRequest
        {
            Resource = $"{_domain}/messages",
            Method = Method.Post
        };
        // parameters
        request.AddParameter("domain", _domain, ParameterType.UrlSegment);
        request.AddParameter("from", message.From!.Email);
        foreach (var to in message.To)
        {
            string toName;
            switch (to.RecipientType)
            {
                case RecipientTypes.Cc:
                    toName = "cc";
                    break;
                case RecipientTypes.Bcc:
                    toName = "bcc";
                    break;
                default:
                    toName = "to";
                    break;
            }
            request.AddParameter(toName, to.Email);
        }
        request.AddParameter("subject", message.Subject);
        request.AddParameter(message.IsHtml ? "html" : "text", message.Body);
        // attachments
        if (message.Attachments != null)
        {
            foreach (var file in message.Attachments)
            {
                var contentType = file.ContentType ?? ContentTypeUtility.GetContentType(file.FileName!);
                request.AddFile("attachment", file.GetBytes()!, file.FileName!, contentType);
            }
        }

        // response
        var mailerResponse = await client.ExecuteAsync(request);

        // errors?
        if (mailerResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new Exception("Not authorized");
        }
        if (!mailerResponse.IsSuccessful)
        {
            throw new Exception("Failed. Status: " + mailerResponse.StatusCode, mailerResponse.ErrorException);
        }

        // result
        var responseMessage = mailerResponse.Content;
        return new MailResponse
        {
            Success = mailerResponse.IsSuccessful,
            Status = mailerResponse.StatusCode.ToString(),
            Content = responseMessage
        };
    }
}