using Regira.Serializing.Abstractions;
using System.Xml.Linq;

namespace Regira.Invoicing.ViaAdValvas;

public class PeppolService
{
    private readonly GatewaySettings _settings;
    private readonly ISerializer _serializer;
    public PeppolService(GatewaySettings settings, ISerializer serializer)
    {
        _settings = settings;
        _serializer = serializer;
    }


    public async Task<UblDocumentResponse> Send(XDocument ublDoc)
    {
        var sendUrl = "document?format=json";
        using var http = new HttpClient
        {
            BaseAddress = new Uri(_settings.Uri),
            DefaultRequestHeaders =
            {
                { "ContentType", "text/plain" }
            }
        };

        var request = CreateRequest(ublDoc);
        // JSON should not be camelCase for AdValVas
        var serializedRequest = _serializer.Serialize(request);
        var httpResponse = await http.PostAsync(sendUrl, new StringContent(serializedRequest));
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new PeppolRequestException("Posting document failed")
            {
                ServiceStatusCode = (int)httpResponse.StatusCode,
                ResponseContent = await httpResponse.Content.ReadAsStringAsync()
            };
        }

        var serializedResponse = await httpResponse.Content.ReadAsStringAsync();

        var response = _serializer.Deserialize<ValidateResponse>(serializedResponse);
        if (response?.Status != true)
        {
            throw new PeppolResponseException("Sending document failed")
            {
                ServiceStatusCode = (int)httpResponse.StatusCode,
                SerializedResponse = serializedResponse
            };
        }
        return CreateResponse(request.MessageReference, response.Status);
    }


    protected UblDocumentRequest CreateRequest(XDocument doc)
    {
        var referenceId = UblDocumentUtility.GetReferenceId(doc);
        var receiverId = $"{UblDocumentUtility.GetRecipientSchemeId(doc)}:{UblDocumentUtility.GetRecipientId(doc)}";
        var date = DateTime.Now;
        var seal = SealUtility.Generate(_settings.SecretKey, _settings.Token, date, _settings.SenderID, referenceId);

        var isCreditNote = doc.Root?.Name.LocalName == "CreditNote";

        using var ms = new MemoryStream();
        doc.Save(ms);
        var bytes = ms.ToArray();

        var request = new UblDocumentRequest
        {
            Channel = "Peppol",
            DateTime = date.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"),
            MessageReference = referenceId,
            ReceiverIdentification = receiverId,
            Seal = seal,
            SenderIdentification = _settings.SenderID,
            SenderName = _settings.SenderName,
            Token = _settings.Token,
            DocumentType = $"{(isCreditNote ? "CreditNote" : "Invoice")} EN-UBL CIUS PEPPOL",
            Document = bytes.Select(b => (int)b).ToArray()
        };

        return request;
    }
    protected UblDocumentResponse CreateResponse(string? reference, bool isSuccess)
    {
        return new UblDocumentResponse
        {
            Reference = reference,
            Success = isSuccess
        };
    }
}